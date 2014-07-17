using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.BoundedContexts.Capi.UI.MaskFormatter;

namespace WB.Core.BoundedContexts.Capi.Tests.UI.MaskedFormatterTests
{
    [Ignore("not implemented yet")]
    [Subject(typeof(MaskedFormatter))]
    internal class when_symbol_located_befoure_literal_symbol_is_deleted
    {
        private Establish context = () =>
        {
            maskedFormatter = new MaskedFormatter(mask);
        };

        private Because of = () =>
            result = maskedFormatter.ValueToString(value, ref cursorPosition);

        private It should_result_be_equal_to_passed_value = () =>
            result.ShouldEqual("w1-234-____");

        private It should_cursor_be_equal_to_6 = () =>
            cursorPosition.ShouldEqual(6);

        private static MaskedFormatter maskedFormatter;
        private static string result;
        private static string mask = "a*-999-a999";
        private static string value = "w1-234____";
        private static int cursorPosition = 6;
    }
}
