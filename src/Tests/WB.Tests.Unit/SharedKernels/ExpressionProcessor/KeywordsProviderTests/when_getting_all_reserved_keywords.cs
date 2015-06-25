using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Tests.Unit.SharedKernels.ExpressionProcessor.KeywordsProviderTests
{
    internal class when_getting_all_reserved_keywords : KeywordsProviderTestContext
    {
        Establish context = () =>
        {
            var substitutionService = CreateSubstitutionService();
            keywordsProvider = CreateKeywordsProvider(substitutionService);

            reservedKeywords = CSharpKeyWords
                .Union(StataVariableRestrictions)
                .Union(SpssReservedKeywords)
                .Union(new[] { substitutionService.RosterTitleSubstitutionReference })
                .Union(SurveySolutionsServiceVariablesKeywords)
                .ToArray();

        };

        Because of = () =>
            result = keywordsProvider.GetAllReservedKeywords();

        It should_contain_fixed_number = () =>
            result.Count().ShouldEqual(reservedKeywords.Count());

        It should_contain_only_predefined_keywords = () =>
            result.ShouldContainOnly(reservedKeywords);

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

        private static readonly List<string> SurveySolutionsServiceVariablesKeywords = new List<string>()
        {
            "rowcode","rowname","rowindex","roster","Id", "self"
        };
        private static string[] reservedKeywords;

        private static IEnumerable<string> result;
        private static IKeywordsProvider keywordsProvider;
    }
}
