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
    internal class when_FormatValue_called_for_text_with_deleted_symbol_before_literal
    {
        Establish context = () =>
        {
            maskedFormatter = new MaskedFormatter(mask);
        };

        Because of = () =>
            result = maskedFormatter.FormatValue(value, ref cursorPosition);

        It should_result_be_equal_to_fomatted_value = () =>
            result.ShouldEqual("qq-q_-___");

        It should_cursor_be_equal_to_4 = () =>
            cursorPosition.ShouldEqual(4);

        private static MaskedFormatter maskedFormatter;
        private static string result;
        private static string mask = "**-**-***";
        private static string value = "qq-q-___";
        private static int cursorPosition = 4;
    }
}
