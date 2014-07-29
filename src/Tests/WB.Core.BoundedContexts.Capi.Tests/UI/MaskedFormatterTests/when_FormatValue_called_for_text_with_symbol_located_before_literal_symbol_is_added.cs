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
    internal class when_FormatValue_called_for_text_with_symbol_located_before_literal_symbol_is_added
    {
        private Establish context = () =>
        {
            maskedFormatter = new MaskedFormatter(mask);
        };

        private Because of = () =>
            result = maskedFormatter.FormatValue(value, ref cursorPosition);

        private It should_result_be_equal_to_formatted_value = () =>
            result.ShouldEqual("w1-234-____");

        private It should_cursor_be_equal_to_7 = () =>
            cursorPosition.ShouldEqual(7);

        private static MaskedFormatter maskedFormatter;
        private static string result;
        private static string mask = "a*-999-a999";
        private static string value = "w1-234_-____";
        private static int cursorPosition = 6;
    }
}
