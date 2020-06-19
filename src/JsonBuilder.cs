﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

            content.AppendLine(@"// auto-generated from api_docify");
            content.AppendLine("var RhinoCommonApi = [");

            // write all namespaces first along with their docs
            // sort namespaces alphabetically
            List<string> namespaceNames = new List<string>(publicTypes.Keys);
            namespaceNames.Sort();
            bool objectWritten = false;
            foreach (var namespaceName in namespaceNames)
            {
                if (!namespaces.ContainsKey(namespaceName))
                    continue; // TODO: figure out these cases
                var nsDefinition = namespaces[namespaceName];
                string jsonType = WriteTypeAsObject(nsDefinition, null);
                if (string.IsNullOrEmpty(jsonType))
                    continue;
                if (objectWritten)
                    content.AppendLine(",");
                content.Append(jsonType);
                objectWritten = true;
            }
            if (objectWritten)
                content.AppendLine(",");

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
            objectWritten = false;
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
            content.AppendLine("export { RhinoCommonApi }");

            System.IO.File.WriteAllText(path, content.ToString());
        }

        static string JsonQuote(string s)
        {
            s = s.Replace("\\", "\\\\");
            if (s.Contains('\n'))
            {
                var lines = s.Split('\n');
                StringBuilder sb = new StringBuilder();
                sb.Append("`");
                for (int i = 0; i < lines.Length; i++)
                {
                    if (i > 0)
                        sb.AppendLine();
                    sb.Append(lines[i].Trim());
                }
                sb.Append("`");
                return sb.ToString();
            }
            s = s.Replace("'", "\\'");
            return "'" + s + "'";
        }

        static string WriteTypeAsObject(ParsedType type, Dictionary<string, ParsedType> allPublicTypesByShortName)
        {
            if (!type.IsPublic || (type.DataType != ParsedDataType.Namespace && string.IsNullOrWhiteSpace(type.Namespace)))
                return null;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("  {");
            if (type.DataType == ParsedDataType.Namespace)
            {
                sb.AppendLine($"    name: '{type.FullName}',");
            }
            else
            {
                sb.AppendLine($"    namespace: '{type.Namespace}',");
                sb.AppendLine($"    name: '{type.Name}',");
            }
            sb.Append($"    dataType: '{type.DataType.ToString().ToLower()}'");
            string summary = type.Summary();
            if( !string.IsNullOrWhiteSpace(summary) )
            {
                sb.AppendLine(",");
                sb.Append($"    summary: {JsonQuote(summary)}");
            }
            string remarks = type.Remarks();
            if (!string.IsNullOrWhiteSpace(remarks))
            {
                sb.AppendLine(",");
                sb.Append($"    remarks: {JsonQuote(remarks)}");
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
                        // TODO: guessing based on .Net naming conventions. I'm sure
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

                if (type.HasSinceTag())
                {
                    sb.AppendLine(",");
                    sb.Append($"    since: '{type.Since}'");
                }
                if (type.HasDeprecatedTag())
                {
                    sb.AppendLine(",");
                    sb.Append($"    deprecated: '{type.Deprecated}'");
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
                    sb.Append($"        since: '{since}'");
                }
                string deprecated = member.Deprecated;
                if (!string.IsNullOrWhiteSpace(deprecated) && double.TryParse(deprecated, out double deprecatedValue))
                {
                    sb.AppendLine(",");
                    sb.Append($"        deprecated: '{deprecated}'");
                }

                var parameters = member.GetParameters();
                if( parameters!=null)
                {
                    // for now, just skip items that have ALL undocumented parameters
                    bool writeParameters = false;
                    for(int i=0; i<parameters.Length; i++)
                    {
                        if( !string.IsNullOrWhiteSpace(parameters[i].DocString))
                        {
                            writeParameters = true;
                            break;
                        }
                    }

                    if (writeParameters)
                    {
                        sb.AppendLine(",");
                        sb.AppendLine($"        parameters: [");
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            if (i > 0)
                                sb.AppendLine(",");
                            sb.AppendLine("          {");
                            sb.AppendLine($"            name: {JsonQuote(parameters[i].Name)},");
                            // Not sure if we really need type as it is easy to resolve in javascript
                            // sb.AppendLine($"            type: {JsonQuote(parameters[i].Type)},");
                            sb.AppendLine($"            summary: {JsonQuote(parameters[i].DocString)}");
                            sb.Append("          }");
                        }
                        sb.AppendLine();
                        sb.Append("        ]");
                    }
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

                if (member.MemberType == ParsedMemberType.Property)
                {
                    bool get, set;
                    if( member.PropertyType(out get, out set))
                    {
                        sb.AppendLine(",");
                        string s = get ? "['get'" : "[";
                        if( set )
                        {
                            if (get)
                                s += ", ";
                            s += "'set'";
                        }
                        s += "]";
                        sb.Append($"        property: {s}");
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

        public static void WriteExamples(
            Dictionary<string, List<ParsedType>> publicTypes,
            string examplesBaseDirectory,
            string outputJsonFile)
        {
            Dictionary<string, List<ParsedMember>> examples = new Dictionary<string, List<ParsedMember>>();
            foreach (var typelist in publicTypes.Values)
            {
                foreach (var type in typelist)
                {
                    if (null == type.Members)
                        continue;
                    foreach (var member in type.Members)
                    {
                        if (member.MemberType == ParsedMemberType.None)
                            continue;
                        string[] sampleRefs = member.GetSampleReferences();
                        if (sampleRefs != null)
                        {
                            for (int i = 0; i < sampleRefs.Length; i++)
                            {
                                string s = sampleRefs[i].ToLower();
                                if (!examples.ContainsKey(s))
                                    examples[s] = new List<ParsedMember>();
                                examples[s].Add(member);
                            }
                        }
                    }
                }
            }

            StringBuilder content = new StringBuilder();
            content.AppendLine("var Examples = [");
            bool addComma = false;
            var keys = new List<string>(examples.Keys);
            // sort so we get a consistent order
            keys.Sort();
            for( int keyIndex=0; keyIndex<keys.Count; keyIndex++ )
            {
                string key = keys[keyIndex];
                var sample = examples[key];
                key = key.Replace('\\', '/');
                string path = System.IO.Path.Combine(examplesBaseDirectory, key);
                string name = System.IO.Path.GetFileName(path);
                if (name.StartsWith("ex_", StringComparison.OrdinalIgnoreCase))
                    name = name.Substring("ex_".Length);
                if (char.IsLower(name[0]))
                    name = char.ToUpper(name[0]) + name.Substring(1);
                string code = System.IO.File.ReadAllText(path);

                code = code.Replace("\\\"", "\"");
                if (addComma)
                    content.AppendLine(",");
                addComma = true;
                content.AppendLine("  {");
                content.AppendLine($"    name: '{name}',");
                content.AppendLine($"    code: `{code}`,");
                content.AppendLine("    members: [");
                for( int i=0; i<sample.Count; i++)
                {
                    if (i > 0)
                        content.AppendLine(",");
                    content.Append($"      ['{sample[i].ParentType.FullName}', '{sample[i].Signature(false)}']");
                }
                content.AppendLine();
                content.AppendLine("    ]");
                content.Append("  }");
            }
            content.AppendLine();
            content.AppendLine("]");
            content.AppendLine();
            content.AppendLine("export { Examples }");
            System.IO.File.WriteAllText(outputJsonFile, content.ToString());
        }
    }
}
