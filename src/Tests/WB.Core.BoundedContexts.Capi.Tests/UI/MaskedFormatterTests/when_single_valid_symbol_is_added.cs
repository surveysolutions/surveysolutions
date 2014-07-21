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
    internal class when_single_valid_symbol_is_added
    {
        Establish context = () =>
        {
            maskedFormatter = new MaskedFormatter(mask);
        };

        Because of = () =>
            result = maskedFormatter.ValueToString(value, ref cursorPosition);

        It should_result_be_equal_to_passed_value = () =>
            result.ShouldEqual("+1(3__)-___");

        It should_cursor_be_equal_to_4 = () =>
            cursorPosition.ShouldEqual(4);

        private static MaskedFormatter maskedFormatter;
        private static string result;
        private static string mask = "+1(999)-999";
        private static string value = "3";
        private static int cursorPosition = 1;
    }
}
