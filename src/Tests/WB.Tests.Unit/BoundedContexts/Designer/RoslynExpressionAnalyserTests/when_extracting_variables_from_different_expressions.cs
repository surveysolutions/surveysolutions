using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;

namespace WB.Tests.Unit.BoundedContexts.Designer.RoslynExpressionAnalyserTests
{
    internal class when_extracting_variables_from_different_expressions
    {
        Establish context = () =>
        {
            expectedResults = new Dictionary<string, IEnumerable<string>>
            {
                { "a+b", new[] { "a", "b" } },
                { "a - b", new[] { "a", "b" } },
                { "x / y)", new[] { "x", "y" } },
                { "(x * y", new[] { "x", "y" } },
                { "((s) + - (t))", new[] { "s", "t" } },
                { "-1", new string[] { } },
                { "w + 7", new[] { "w" } },
                { "r + r - r", new[] { "r" } },
            };

            analyzer = Create.RoslynExpressionAnalyser();
        };

        Because of = () =>
            results = expectedResults.Keys.ToDictionary(
                expression => expression,
                expression => analyzer.ExtractVariables(expression));

        It should_return_only_expected_variables = () =>
        {
            foreach (var result in results)
            {
                result.Value.ShouldContainOnly(expectedResults[result.Key]);
            }
        };

        private static Dictionary<string, IEnumerable<string>> results;
        private static Dictionary<string, IEnumerable<string>> expectedResults;
        private static RoslynExpressionAnalyser analyzer;
    }
}