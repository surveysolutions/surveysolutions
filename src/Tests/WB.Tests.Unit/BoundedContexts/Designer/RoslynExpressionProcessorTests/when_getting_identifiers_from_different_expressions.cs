using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;

namespace WB.Tests.Unit.BoundedContexts.Designer.RoslynExpressionProcessorTests
{
    internal class when_getting_identifiers_from_different_expressions
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
                { "c1 + \"s\"", new[] { "c1" } },
                { "c2 + 's'", new[] { "c2" } },
                { "mo1.Contains(1)", new[] { "mo1" } },
                { "a.b() + x.y(z) + k(l) + w()", new[] { "a", "x", "z", "l" } },
                { "a.x", new[] { "a" } },
                { "x_date > new DateTime(2014, 08, 19)", new[] { "x_date" } },
            };

            analyzer = Create.RoslynExpressionProcessor();
        };

        Because of = () =>
            results = expectedResults.Keys.ToDictionary(
                expression => expression,
                expression => analyzer.GetIdentifiersUsedInExpression(expression));

        It should_return_only_expected_identifiers = () =>
        {
            foreach (var result in results)
            {
                result.Value.ShouldContainOnly(expectedResults[result.Key]);
            }
        };

        private static Dictionary<string, IEnumerable<string>> results;
        private static Dictionary<string, IEnumerable<string>> expectedResults;
        private static RoslynExpressionProcessor analyzer;
    }
}