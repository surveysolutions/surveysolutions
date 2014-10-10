using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;

namespace WB.Tests.Unit.BoundedContexts.Designer.NCalcToRoslynConverterTests
{
    internal class when_converting_different_ncalc_expressions_to_roslyn
    {
        Establish context = () =>
        {
            expectedResults = new Dictionary<string, string>
            {
                { "[a] > 0", "a > 0" },
                { "[x] = [y]", "x == y" },
            };

            converter = Create.NCalcToRoslynConverter();
        };

        Because of = () =>
            results = expectedResults.Keys.Select(converter.Convert).ToArray();

        It should_return_only_expected_expressions = () =>
            results.ShouldEqual(expectedResults.Values.ToArray());

        private static string[] results;
        private static Dictionary<string, string> expectedResults;
        private static NCalcToRoslynConverter converter;
    }
}