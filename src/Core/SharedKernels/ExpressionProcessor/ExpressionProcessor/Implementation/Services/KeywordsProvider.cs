using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.ExpressionProcessor.Services;

namespace WB.Core.SharedKernels.ExpressionProcessor.Implementation.Services
{
    public class KeywordsProvider : IKeywordsProvider
    {
        public KeywordsProvider(ISubstitutionService substitutionService)
        {
            this.substitutionService = substitutionService;
        }

        private readonly ISubstitutionService substitutionService;

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

        private static readonly List<string> ReservedKeywords =
            CSharpKeyWords.Union(StataVariableRestrictions).Union(SpssReservedKeywords).ToList();

        public string[] GetAllReservedKeywords()
        {
            var reservedKeywords = ReservedKeywords.ToList();
            reservedKeywords.Add(substitutionService.RosterTitleSubstitutionReference);
            return ReservedKeywords.Distinct().ToArray();
        }
    }
}