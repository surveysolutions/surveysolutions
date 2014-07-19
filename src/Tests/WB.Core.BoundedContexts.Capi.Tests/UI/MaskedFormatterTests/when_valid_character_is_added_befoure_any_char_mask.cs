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
    internal class when_valid_character_is_added_befoure_any_char_mask
    {
        Establish context = () =>
        {
            maskedFormatter = new MaskedFormatter(mask);
        };

        Because of = () =>
            result = maskedFormatter.ValueToString(value, ref cursorPosition);

        It should_result_be_equal_to_passed_value = () =>
            result.ShouldEqual("b_-___-____");

        It should_cursor_be_equal_to_1 = () =>
            cursorPosition.ShouldEqual(1);

        private static MaskedFormatter maskedFormatter;
        private static string result;
        private static string mask = "a*-999-a999";
        private static string value = "b__-___-____";
        private static int cursorPosition = 1;
    }
}
