﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace api_docify
{
    class JsonBuilder
    {
        public static void Write(
            Dictionary<string, ParsedType> namespaces,
            Dictionary<string, List<ParsedType>> publicTypes,
            string path)
        {
            StringBuilder content = new StringBuilder();

            content.Append(@"// auto-generated from api_docify
const DataTypes = {
  NONE: 0,
  CLASS: 1,
  STRUCT: 2,
  ENUM: 3,
  INTERFACE: 4,
  NAMESPACE: 5
}
");
            content.AppendLine("var RhinoCommonApi = [");
            bool objectWritten = false;

            // write all namespaces first along with their docs
            foreach (var kv in publicTypes)
            {
                string namespaceName = kv.Key;
                if (!namespaces.ContainsKey(namespaceName))
                    continue; // TODO: figure out these cases
                var nsDefinition = namespaces[kv.Key];
                string jsonType = WriteTypeAsObject(nsDefinition, null);
                if (string.IsNullOrEmpty(jsonType))
                    continue;
                if (objectWritten)
                    content.AppendLine(",");
                content.Append(jsonType);
                objectWritten = true;
            }

            List<ParsedType> sortedTypes = new List<ParsedType>();
            foreach(var kv in publicTypes)
            {
                sortedTypes.AddRange(kv.Value);
            }
            sortedTypes.Sort((a, b) => { return a.FullName.CompareTo(b.FullName); });
            var typesByNameDictionary = new Dictionary<string, ParsedType>();
            foreach(var type in sortedTypes)
            {
                string name = type.Name;
                if( !name.Equals("EventArgs"))
                    typesByNameDictionary[name] = type;
            }

            for( int i=0; i<sortedTypes.Count; i++ )
            {
                string jsonType = WriteTypeAsObject(sortedTypes[i], typesByNameDictionary);
                if (string.IsNullOrEmpty(jsonType))
                    continue;
                if (objectWritten)
                    content.AppendLine(",");
                content.Append(jsonType);
                objectWritten = true;
            }
            content.AppendLine();
            content.AppendLine("]");
            content.AppendLine();
            content.AppendLine("export { DataTypes, RhinoCommonApi }");

            System.IO.File.WriteAllText(path, content.ToString());
        }

        static string JsonQuote(string s)
        {
            s = s.Replace("\\", "\\\\");
            if (s.Contains('\n'))
                return "`" + s + "`";
            s = s.Replace("'", "\\'");
            return "'" + s + "'";
        }

        static string WriteTypeAsObject(ParsedType type, Dictionary<string, ParsedType> allPublicTypesByShortName)
        {
            if (!type.IsPublic)
                return null;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("  {");
            sb.AppendLine($"    name: '{type.FullName}',");
            sb.Append($"    dataType: {(int)(type.DataType)}");
            string summary = type.Summary();
            if( !string.IsNullOrWhiteSpace(summary) )
            {
                sb.AppendLine(",");
                sb.Append($"    summary: {JsonQuote(summary)}");
            }

            if (type.DataType == ParsedDataType.Namespace)
            {
                sb.AppendLine();
            }
            else
            {
                string[] baseList = type.IsClass ? type.GetBaseList(allPublicTypesByShortName) : null;
                if (baseList != null && baseList.Length > 0)
                {
                    sb.AppendLine(",");
                    int firstInterfaceIndex = -1;
                    for (int i=0; i<baseList.Length; i++)
                    {
                        // guessing based on .Net naming conventions. I'm sure
                        // this can be improved
                        if(baseList[i].StartsWith("I") && char.IsUpper(baseList[i][1]))
                        {
                            firstInterfaceIndex = i;
                            break;
                        }
                    }
                    if(firstInterfaceIndex!=0)
                    {
                        sb.Append($"    baseclass: {JsonQuote(baseList[0])}");
                    }
                    if(firstInterfaceIndex>-1)
                    {
                        if( firstInterfaceIndex>0)
                            sb.AppendLine(",");
                        sb.Append("    interfaces: [");
                        for (int i = firstInterfaceIndex; i < baseList.Length; i++)
                        {
                            if (i > firstInterfaceIndex)
                                sb.Append(", ");
                            sb.Append(JsonQuote(baseList[i]));
                        }
                        sb.Append("]");
                    }
                }

                string values = MembersAsJsonArray(type, ParsedMemberType.EnumValue);
                string constructors = MembersAsJsonArray(type, ParsedMemberType.Constructor);
                string properties = MembersAsJsonArray(type, ParsedMemberType.Property);
                string methods = MembersAsJsonArray(type, ParsedMemberType.Method);
                string events = MembersAsJsonArray(type, ParsedMemberType.Event);
                if (values != null || constructors != null || properties != null || methods != null || events != null)
                    sb.AppendLine(",");
                else
                    sb.AppendLine();

                if (!string.IsNullOrWhiteSpace(values))
                {
                    sb.AppendLine($"    values: {values}");
                }
                if (!string.IsNullOrWhiteSpace(constructors))
                {
                    sb.Append($"    constructors: {constructors}");
                    if (properties != null || methods != null || events != null)
                        sb.AppendLine(",");
                    else
                        sb.AppendLine();
                }
                if (!string.IsNullOrWhiteSpace(properties))
                {
                    sb.Append($"    properties: {properties}");
                    if (methods != null || events != null)
                        sb.AppendLine(",");
                    else
                        sb.AppendLine();
                }
                if (!string.IsNullOrWhiteSpace(methods))
                {
                    sb.Append($"    methods: {methods}");
                    if (events != null)
                        sb.AppendLine(",");
                    else
                        sb.AppendLine();
                }
                if (!string.IsNullOrWhiteSpace(events))
                    sb.AppendLine($"    events: {events}");
            }
            sb.Append("  }");
            return sb.ToString();
        }

        static string MembersAsJsonArray(ParsedType type, ParsedMemberType filter)
        {
            if (type.Members == null)
                return null;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[");
            bool memberAdded = false;
            foreach (var member in type.Members)
            {
                if (filter != member.MemberType)
                    continue;
                if (memberAdded)
                    sb.AppendLine(",");
                sb.AppendLine("      {");
                sb.Append($"        signature: '{member.Signature(false)}'");
                string summary = member.Summary();
                if (!string.IsNullOrWhiteSpace(summary))
                {
                    sb.AppendLine(",");
                    sb.Append($"        summary: {JsonQuote(summary)}");
                }
                string since = member.Since;
                if (!string.IsNullOrWhiteSpace(since) && double.TryParse(since, out double sinceValue))
                {
                    sb.AppendLine(",");
                    sb.Append($"        since: {since}");
                }

                if (member.MemberType == ParsedMemberType.Method)
                {
                    string returns = member.ReturnDocString();
                    if (!string.IsNullOrWhiteSpace(returns))
                    {
                        sb.AppendLine(",");
                        sb.Append($"        returns: {JsonQuote(returns)}");
                    }
                }
                sb.AppendLine();
                sb.Append("      }");
                memberAdded = true;
            }
            sb.AppendLine();
            sb.Append("    ]");

            return memberAdded ? sb.ToString() : null;
        }
    }
}
