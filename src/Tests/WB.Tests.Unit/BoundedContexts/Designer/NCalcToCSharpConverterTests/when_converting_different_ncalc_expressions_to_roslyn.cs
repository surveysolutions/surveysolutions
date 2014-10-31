using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;

namespace WB.Tests.Unit.BoundedContexts.Designer.NCalcToCSharpConverterTests
{
    internal class when_converting_different_ncalc_expressions_to_roslyn
    {
        Establish context = () =>
        {
            expectedResults = new Dictionary<string, string>
            {
                { "[a] > 0", "a > 0" },
                { "[a] > 0 and [b] < 3", "(a > 0) && (b < 3)" },
                { "[x] = [y]", "x == y" },
                { "contains([prob_school],8)", "prob_school.Contains(8)" },
                { "in([q],1,2,3)", "new decimal[] { 1, 2, 3 }.Contains(q)" },
                { "in([q],1)", "q == 1" },
            };

            converter = Create.NCalcToCSharpConverter();
        };

        Because of = () =>
            results = expectedResults
                .Keys
                .Select(ncalcExpression => converter.Convert(ncalcExpression, customMappings: null))
                .ToArray();

        It should_return_only_expected_expressions = () =>
            results.ShouldEqual(expectedResults.Values.ToArray());

        private static string[] results;
        private static Dictionary<string, string> expectedResults;
        private static NCalcToCSharpConverter converter;
    }
}