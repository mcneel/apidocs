﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace api_docify
{
    /// <summary>
    /// Class, struct, enum, or interface declaration
    /// </summary>
    class ParsedType : XmlDocumentedItem
    {
        BaseTypeDeclarationSyntax _basetype;
        public ParsedType(BaseTypeDeclarationSyntax basetype, DocumentationCommentTriviaSyntax documentation) : base(documentation)
        {
            _basetype = basetype;
        }

        public void Merge(ParsedType other)
        {
            if (!FullName.Equals(other.FullName))
                throw new Exception("Invalid Merge");
            if( other.Documentation!= null )
            {
                // TODO: deal with merging documentation
                //if (this.Documentation != null)
                //    throw new Exception("two Documentation sections on merge");
                Documentation = other.Documentation;
            }
        }

        public bool IsClass { get { return _basetype is ClassDeclarationSyntax; } }

        public string FullName
        {
            get
            {
                return GetFullContainerName(_basetype);
            }
        }

        public string[] BaseTypes()
        {
            var types = _basetype.BaseList.Types;
            string[] rc = new string[types.Count];
            for( int i=0; i<types.Count; i++)
            {
                rc[i] = types[i].ToString();
            }
            return rc;
        }

        public string Name
        {
            get
            {
                return $"{_basetype.Identifier}";
            }
        }

        public string Namespace
        {
            get
            {
                string ns = "";
                var parent = _basetype.Parent;
                while (parent != null)
                {
                    var namespaceDeclaration = parent as NamespaceDeclarationSyntax;
                    if (namespaceDeclaration != null)
                    {
                        ns = $"{namespaceDeclaration.Name}.{ns}";
                    }
                    parent = parent.Parent;
                }
                return ns.TrimEnd(new char[] { '.' });
            }
        }

        public bool IsPublic
        {
            get
            {
                return _basetype.IsPublic();
            }
        }

        public List<ParsedMember> Members { get; set; }

        public static string GetFullContainerName(BaseTypeDeclarationSyntax basetype)
        {
            string ns = "";
            var parent = basetype.Parent;
            while (parent != null)
            {
                var parentClassDeclaration = parent as ClassDeclarationSyntax;
                if (parentClassDeclaration != null)
                {
                    ns = $"{parentClassDeclaration.Identifier}.{ns}";
                }
                var namespaceDeclaration = parent as NamespaceDeclarationSyntax;
                if (namespaceDeclaration != null)
                {
                    ns = $"{namespaceDeclaration.Name}.{ns}";
                }
                parent = parent.Parent;
            }

            string className = $"{ns}{basetype.Identifier}";
            return className;
        }


        public string[] GetAttributes()
        {
            string[] rc = new string[_basetype.AttributeLists.Count];
            for (int i = 0; i < rc.Length; i++)
                rc[i] = _basetype.AttributeLists[i].ToString();
            return rc;
        }

        public string GetBaseList()
        {
            if (_basetype.BaseList != null)
                return _basetype.BaseList.ToString();
            return "";
        }
    }
}
