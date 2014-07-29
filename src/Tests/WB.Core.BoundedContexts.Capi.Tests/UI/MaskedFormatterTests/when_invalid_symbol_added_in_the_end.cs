using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.BoundedContexts.Capi.UI.MaskFormatter;

namespace WB.Core.BoundedContexts.Capi.Tests.UI.MaskedFormatterTests
{
    [Subject(typeof(MaskedFormatter))]
    class when_invalid_symbol_added_in_the_end
    {
        Establish context = () =>
        {
            maskedFormatter = new MaskedFormatter(mask);
        };

        Because of = () =>
            result = maskedFormatter.FormatValue(value, ref cursorPosition);

        It should_result_be_equal_to_passed_value = () =>
            result.ShouldEqual(value);

        It should_cursor_be_equal_to_11 = () =>
            cursorPosition.ShouldEqual(11);

        private static MaskedFormatter maskedFormatter;
        private static string result;
        private static string mask = "a*-999-a999";
        private static string value = "w1-234-a567";
        private static int cursorPosition = 11;
    }
}
