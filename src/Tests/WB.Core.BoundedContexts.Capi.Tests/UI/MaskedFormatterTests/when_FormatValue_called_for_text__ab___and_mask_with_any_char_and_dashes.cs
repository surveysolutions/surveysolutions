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
    internal class when_FormatValue_called_for_text__ab___and_mask_with_any_char_and_dashes
    {
        Establish context = () =>
        {
            maskedFormatter = new MaskedFormatter(mask);
        };

        Because of = () =>
            result = maskedFormatter.FormatValue(value, ref cursorPosition);

        It should_result_be_equal_to_fomatted_value = () =>
            result.ShouldEqual("qw-__-___");

        It should_cursor_be_equal_to_3 = () =>
            cursorPosition.ShouldEqual(3);

        private static MaskedFormatter maskedFormatter;
        private static string result;
        private static string mask = "**-**-***";
        private static string value = "qw_-__-___";
        private static int cursorPosition = 2;
    }
}
