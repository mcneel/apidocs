﻿using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace api_docify
{
    enum ParsedMemberType
    {
        None = 0,
        Constructor = 1,
        Event = 2,
        Property = 3,
        Method = 4,
        EnumValue = 5
    }

    /// <summary>
    /// Things "inside" of a class, struct, or interface. This includes
    /// constructors, events, properties, or methods
    /// </summary>
    class ParsedMember : XmlDocumentedItem
    {
        public ParsedMember(MemberDeclarationSyntax member, DocumentationCommentTriviaSyntax documentation)
            : base (documentation)
        {
            Member = member;
            ParsedMemberType mt = ParsedMemberType.None;
            if (Member is ConstructorDeclarationSyntax) mt = ParsedMemberType.Constructor;
            if (Member is EventDeclarationSyntax) mt = ParsedMemberType.Event;
            if (Member is PropertyDeclarationSyntax) mt = ParsedMemberType.Property;
            if (Member is MethodDeclarationSyntax) mt = ParsedMemberType.Method;
            if (Member is EnumMemberDeclarationSyntax) mt = ParsedMemberType.EnumValue;
            MemberType = mt;
        }
        public ParsedType ParentType { get; set; }
        private MemberDeclarationSyntax Member { get; }

        public int SinceInsertIndex()
        {
            int rc = Member.SpanStart;
            return rc;
        }

        public ParsedMemberType MemberType { get; }

        public bool IsConstructor { get { return Member is ConstructorDeclarationSyntax; } }
        public bool IsEvent { get { return Member is EventDeclarationSyntax; } }
        public bool IsProperty { get { return Member is PropertyDeclarationSyntax; } }
        public bool IsMethod { get { return Member is MethodDeclarationSyntax; } }

        public bool ParentIsPublic
        {
            get
            {
                var parent = Member.Parent;
                while (parent != null)
                {
                    if (parent is NamespaceDeclarationSyntax)
                        return true;
                    var parentClass = parent as ClassDeclarationSyntax;
                    if (parentClass != null && !parentClass.IsPublic())
                        return false;
                    var interfaceDeclaration = parent as InterfaceDeclarationSyntax;
                    if (interfaceDeclaration != null && !interfaceDeclaration.IsPublic())
                        return false;
                    var structDeclaration = parent as StructDeclarationSyntax;
                    if (structDeclaration != null && !structDeclaration.IsPublic())
                        return false;

                    parent = parent.Parent;
                }
                return true;
            }
        }

        public bool IsPublic { get { return Member.IsPublic(); } }
        public bool IsStatic { get { return Member.IsStatic(); } }

        public string ClassPath
        {
            get
            {
                return ParsedType.GetFullContainerName(Member.Parent as BaseTypeDeclarationSyntax);
            }
        }

        public bool PropertyType(out bool get, out bool set)
        {
            get = false;
            set = false;
            PropertyDeclarationSyntax property = Member as PropertyDeclarationSyntax;
            if (property != null)
            {
                if (property.AccessorList == null)
                {
                    get = true;
                }
                else
                {
                    for (int i = 0; i < property.AccessorList.Accessors.Count; i++)
                    {
                        var accessor = property.AccessorList.Accessors[i].Keyword.Text;
                        if (accessor.Equals("get"))
                            get = true;
                        if (accessor.Equals("set"))
                            set = true;
                    }
                }
                return true;
            }
            return false;
        }

        public string Signature(bool forSorting)
        {
            string prefix = forSorting ? ClassPath + "." : "";
            if (!forSorting && IsStatic)
                prefix = "static ";

            {
                MethodDeclarationSyntax method = Member as MethodDeclarationSyntax;
                if (method != null)
                {
                    var signature = new System.Text.StringBuilder();
                    if (forSorting)
                    {
                        signature.Append($"{ClassPath}.{method.Identifier}(");
                    }
                    else
                    {
                        signature.Append($"{prefix}{method.ReturnType} {method.Identifier}(");
                    }
                    int parameterCount = method.ParameterList.Parameters.Count;
                    for (int i = 0; i < parameterCount; i++)
                    {
                        if (i > 0)
                            signature.Append(",");
                        var parameter = method.ParameterList.Parameters[i];
                        string paramType = parameter.Type.ToString();
                        int angleIndex = paramType.IndexOf('<');
                        string prefixType = "";
                        if (angleIndex > 0)
                        {
                            prefixType = paramType.Substring(0, angleIndex);
                            int prefixIndex = prefixType.LastIndexOf('.');
                            if (prefixIndex > 0)
                                prefixType = prefixType.Substring(prefixIndex + 1);
                            string genericType = paramType.Substring(angleIndex + 1);
                            int genericIndex = genericType.LastIndexOf('.');
                            if (genericIndex > 0)
                                genericType = genericType.Substring(genericIndex + 1);
                            paramType = prefixType + "<" + genericType;
                        }
                        else
                        {
                            int index = paramType.LastIndexOf('.');
                            if (index > 0)
                                paramType = paramType.Substring(index + 1);
                        }
                        signature.Append(paramType);
                        if (!forSorting)
                        {
                            signature.Append($" {parameter.Identifier}");
                        }
                    }
                    signature.Append(")");
                    return signature.ToString();
                }
            }
            {
                PropertyDeclarationSyntax property = Member as PropertyDeclarationSyntax;
                if (property != null)
                {
                    var signature = new System.Text.StringBuilder();
                    if (forSorting)
                    {
                        signature.Append($"{ClassPath}.{property.Identifier}");
                    }
                    else
                    {
                        string proptype = $"{property.Type}";
                        int index = proptype.LastIndexOf('.');
                        if (index > 0)
                            proptype = proptype.Substring(index + 1);
                        signature.Append($"{prefix}{proptype} {property.Identifier}");
                    }
                    return signature.ToString();
                }
            }
            {
                EventDeclarationSyntax evt = Member as EventDeclarationSyntax;
                if (evt != null)
                {
                    var signature = new System.Text.StringBuilder();
                    signature.Append($"{prefix}{evt.Identifier}");
                    return signature.ToString();
                }
            }
            {
                OperatorDeclarationSyntax op = Member as OperatorDeclarationSyntax;
                if (op != null)
                {
                    var signature = new System.Text.StringBuilder();
                    signature.Append($"{prefix}{op.OperatorToken}");
                    return signature.ToString();
                }
            }
            {
                EventFieldDeclarationSyntax eventField = Member as EventFieldDeclarationSyntax;
                if (eventField != null)
                {
                    var signature = new System.Text.StringBuilder();
                    string declaration = eventField.ToString();
                    int index = declaration.LastIndexOf(' ');
                    declaration = declaration.Substring(index + 1, declaration.Length - 1 - (index + 1));
                    signature.Append($"{prefix}{declaration}");
                    return signature.ToString();
                }
            }
            {
                ConstructorDeclarationSyntax constructor = Member as ConstructorDeclarationSyntax;
                if (constructor != null)
                {
                    var signature = new System.Text.StringBuilder();
                    if (forSorting)
                    {
                        signature.Append($"{ClassPath}(");
                    }
                    else
                    {
                        if (IsStatic)
                            signature.Append("static ");
                        var parent = new ParsedType(Member.Parent as BaseTypeDeclarationSyntax, null);
                        signature.Append($"{parent.Name}(");
                    }
                    int parameterCount = constructor.ParameterList.Parameters.Count;
                    for (int i = 0; i < parameterCount; i++)
                    {
                        if (i > 0)
                            signature.Append(",");
                        var parameter = constructor.ParameterList.Parameters[i];
                        string paramType = parameter.Type.ToString();
                        int angleIndex = paramType.IndexOf('<');
                        string prefixType = "";
                        if (angleIndex > 0)
                        {
                            prefixType = paramType.Substring(0, angleIndex);
                            int prefixIndex = prefixType.LastIndexOf('.');
                            if (prefixIndex > 0)
                                prefixType = prefixType.Substring(prefixIndex + 1);
                            string genericType = paramType.Substring(angleIndex + 1);
                            int genericIndex = genericType.LastIndexOf('.');
                            if (genericIndex > 0)
                                genericType = genericType.Substring(genericIndex + 1);
                            paramType = prefixType + "<" + genericType;
                        }
                        else
                        {
                            int index = paramType.LastIndexOf('.');
                            if (index > 0)
                                paramType = paramType.Substring(index + 1);
                        }
                        signature.Append(paramType);
                        if (!forSorting)
                        {
                            signature.Append($" {parameter.Identifier}");
                        }
                    }
                    signature.Append(")");
                    return signature.ToString();
                }
            }
            {
                EnumMemberDeclarationSyntax enumMember = Member as EnumMemberDeclarationSyntax;
                if (enumMember != null)
                {
                    var signature = enumMember.ToString();
                    var items = signature.Split(new char[] { '\n' });
                    signature = items[items.Length - 1];
                    return signature;
                }
            }
            throw new System.NotImplementedException();
        }

        public ParsedType ReturnType(IEnumerable<ParsedType> types)
        {
            string searchName = null;
            {
                PropertyDeclarationSyntax property = Member as PropertyDeclarationSyntax;
                if (property != null)
                {
                    string proptype = $"{property.Type}";
                    int index = proptype.LastIndexOf('.');
                    if (index > 0)
                        proptype = proptype.Substring(index + 1);
                    searchName = proptype;
                }
            }
            {
                MethodDeclarationSyntax method = Member as MethodDeclarationSyntax;
                if (method != null)
                {
                    searchName = $"{method.ReturnType}";
                }
            }
            if (string.IsNullOrEmpty(searchName))
                return null;
            if (char.IsLower(searchName[0]))
                return null;
            if (searchName.EndsWith("[]"))
                searchName = searchName.Substring(0, searchName.Length - 2);

            foreach (var type in types)
            {
                if( type.Name.EndsWith(searchName) )
                {
                    string name = type.Name;
                    int index = name.LastIndexOf('.');
                    if (index > 0)
                        name = name.Substring(index + 1);
                    if (searchName.Equals(name))
                        return type;
                }
            }
            return null;
        }

        public string Returns()
        {
            string rc = "";
            {
                MethodDeclarationSyntax method = Member as MethodDeclarationSyntax;
                if (method != null)
                {
                    rc = $"{method.ReturnType} This is some return comment";
                }
            }
            return rc;
        }
    }
}
