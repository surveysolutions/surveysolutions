using System.Collections.Generic;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Implementation.Services;

namespace WB.Tests.Unit.BoundedContexts.Designer.NCalcToRoslynConverterTests
{
    internal class when_converting__a_plus_b_gt_c_plus_d__to_roslyn_and_a_is_mapped_to_x_and_b_is_mapped_to_y
    {
        Establish context = () =>
        {
            converter = Create.NCalcToRoslynConverter();
        };

        Because of = () =>
            result = converter.Convert(
                "a + b > c + d",
                customMappings: new Dictionary<string, string>
                {
                    { "a", "x" },
                    { "b", "y" },
                });

        It should_return__x_plus_y_gt_c_plus_d__ = () =>
            result.ShouldEqual("(x + y) > (c + d)");

        private static string result;
        private static NCalcToRoslynConverter converter;
    }
}