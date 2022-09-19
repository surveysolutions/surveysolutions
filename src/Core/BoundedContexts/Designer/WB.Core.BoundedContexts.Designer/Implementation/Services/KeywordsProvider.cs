using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    internal class KeywordsProvider : IKeywordsProvider
    {
        public KeywordsProvider(ISubstitutionService substitutionService)
        {
            this.reservedKeywords = CSharpKeyWords
                .Union(StataVariableRestrictions)
                .Union(SpssReservedKeywords)
                .Union(new[] { substitutionService.RosterTitleSubstitutionReference })
                .Union(SurveySolutionsServiceVariablesKeywords)
                .Union(WindowsFileNamingReservedNames)
                .Union(ServiceColumns.SystemVariables.Values.Select(x=>x.VariableExportColumnName))
                .Select(x => x.ToLower())
                .ToArray();
        }

        private readonly string[] reservedKeywords;

        private static readonly List<string> CSharpKeyWords = new List<string>()
        {
            "abstract", "as", "base", "bool", "break", "byte", "case",
            "catch", "char", "checked", "class", "const", "continue", "decimal",
            "default", "delegate", "do", "double", "else", "enum", "event",
            "explicit", "extern", "false", "finally", "fixed", "float", "for",
            "foreach", "goto", "if", "implicit", "in", "int", "interface",
            "internal", "is", "lock", "long", "namespace", "new", "null",
            "object", "operator", "out", "override", "params", "private", "protected",
            "public", "readonly", "ref", "return", "sbyte", "sealed", "short",
            "sizeof", "stackalloc", "static", "string", "struct", "switch", "this",
            "throw", "true", "try", "typeof", "uint", "ulong", "unchecked",
            "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
            
        };

        private static readonly List<string> StataVariableRestrictions = new List<string>()
        {
            "_all", "_b", "byte", "_coef", "_cons", "double", "float", "if", "in", "int", "long", "_n", "_pi",
            "_pred", "_rc", "_skip", "strl", "using", "with"
        };

        private static readonly List<string> SpssReservedKeywords = new List<string>()
        {
            "all", "and", "by", "eq", "ge", "gt", "le", "lt", "ne", "not", "or", "to", "with"
        };

        private static readonly List<string> SurveySolutionsServiceVariablesKeywords = new List<string>()
        {
            "rowcode","rowname","rowindex","roster","id", "parentid1", "parentid2", "parentid3", "parentid4",
            "self", "state", "quest", "optioncode", "complete", "cover", "overview", "questionnaire"
        };

        private static readonly List<string> WindowsFileNamingReservedNames = new List<string>()
        {
            "con", "prn", "aux", "nul", "com1", "com2", "com3", "com4", "com5", "com6", "com7", "com8","com9",
            "lpt1", "lpt2","lpt3", "lpt4", "lpt5", "lpt6", "lpt7", "lpt8", "lpt9"
        };

        public bool IsReservedKeyword(string keyword)
        {
            if (string.IsNullOrEmpty(keyword)) return false;

            keyword = keyword.ToLower();

            if (this.reservedKeywords.Contains(keyword))
                return true;

            if (Regex.IsMatch(keyword, @"^str\d+"))
                return true;

            return false;
        }
    }
}
