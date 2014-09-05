using System.Linq;
using WB.Core.SharedKernels.ExpressionProcessor.Services;

namespace WB.Core.SharedKernels.ExpressionProcessor.Implementation.Services
{
    public class VariableNameValidator : IVariableNameValidator
    {
        private static readonly string[] CSharpKeyWords = new[]
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

        private static readonly string[] StataVariableRestrictions = new[]
        {
            "_all", "_b", "byte", "_coef", "_cons", "double", "float", "if", "in", "int", "long", "_n", "_pi",
            "_pred", "_rc", "_skip", "strl", "using", "with"
        };

        private static readonly string[] SpssReservedKeywords = new[]
        {
            "all", "and", "by", "eq", "ge", "gt", "le", "lt", "ne", "not", "or", "to", "with"
        };

        private static readonly string[] ReservedKeywords =
            CSharpKeyWords.Union(StataVariableRestrictions).Union(SpssReservedKeywords).ToArray();

        public string[] GetAllReservedKeywords()
        {
            return ReservedKeywords;
        }
    }
}