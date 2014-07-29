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
    internal class when_FormatValue_called_for_text_with_symbol_added_at_position_befoure_it_became_valid
    {
        Establish context = () =>
        {
            maskedFormatter = new MaskedFormatter(mask);
        };

        Because of = () =>
            result = maskedFormatter.FormatValue(value, ref cursorPosition);

        It should_result_be_equal_to_formatted_value = () =>
            result.ShouldEqual("w2-123-____");

        It should_cursor_be_equal_to_7 = () =>
            cursorPosition.ShouldEqual(7);

        private static MaskedFormatter maskedFormatter;
        private static string result;
        private static string mask = "a*-999-a999";
        private static string value = "w2-123-5____";
        private static int cursorPosition = 8;
    }
}
