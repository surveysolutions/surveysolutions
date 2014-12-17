using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.MaskFormatter;

namespace WB.Tests.Unit.BoundedContexts.Capi.MaskedFormatterTests
{
    [Subject(typeof(MaskedFormatter))]
    internal class when_answer_is_1_and_mask_has_8_symbols
    {
        Establish context = () =>
        {
            maskedFormatter = new MaskedFormatter(mask);
        };

        Because of = () => formatted = maskedFormatter.FormatValue("1", ref cursorPosition);

        It should_return_formatted_string = () => formatted.ShouldEqual("1_______");

        It should_cursor_be_equal_to_1 = () =>
           cursorPosition.ShouldEqual(1);

        private static string mask = "########";
        private static MaskedFormatter maskedFormatter;
        private static string formatted;
        private static int cursorPosition = 0;
    }
}
