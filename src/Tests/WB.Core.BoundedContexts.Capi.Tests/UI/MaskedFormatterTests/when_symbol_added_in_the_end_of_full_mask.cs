using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.BoundedContexts.Capi.UI.MaskFormatter;

namespace WB.Core.BoundedContexts.Capi.Tests.UI.MaskedFormatterTests
{
    [Subject(typeof (MaskedFormatter))]
    class when_symbol_added_in_the_end_of_full_mask
    {
        private Establish context = () =>
        {
            maskedFormatter = new MaskedFormatter(mask);
        };

        private Because of = () =>
            result = maskedFormatter.FormatValue(value+"x", ref cursorPosition);

        private It should_result_be_equal_to_passed_value = () =>
            result.ShouldEqual(value);

        private It should_cursor_be_equal_to_11 = () =>
            cursorPosition.ShouldEqual(11);

        private static MaskedFormatter maskedFormatter;
        private static string result;
        private static string mask = "a*-999-a999";
        private static string value = "w1-234-a567";
        private static int cursorPosition = 12;
    }
}
