using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.RoslynExpressionProcessorTests
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
                { "mo1.Contains(1)", new[] { "mo1", "Contains" } },
                { "a.b() + x.y(z) + k(l) + w()", new[] { "a", "b", "x", "y", "z", "l" } },
                { "a.x", new[] { "a", "x"} },
                { "x_date > new DateTime(2014, 08, 19)", new[] { "x_date" } },
                { "if then else", new[] { "then" } },
                { "roster.Select(x => x.age).Max()", new[] { "roster", "age", "Select", "Max" } },
                { "roster.Select((x, i) => x.age).Max()", new[] { "roster", "age", "Select", "Max" } },
                { "roster.Select(y => new { age2 = y.age*100 }).Select(x => x.age2).Max()", new[] { "roster", "age", "age2", "Select", "Max" } },
                { "roster.Select(x => x.gps.Latitude).Max()", new[] { "roster", "gps", "Select", "Max", "Latitude" } },

                { "roster[1].variable", new[] { "roster", "variable" } },
                { "roster.First().variable", new[] { "roster", "variable", "First" } },
                { "roster.First().roster1.First().gps.Latitude > 0", new[] { "roster", "roster1", "gps", "First", "Latitude" } }
                

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