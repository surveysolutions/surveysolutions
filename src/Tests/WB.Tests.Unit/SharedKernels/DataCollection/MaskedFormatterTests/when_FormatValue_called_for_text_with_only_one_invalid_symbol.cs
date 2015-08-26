using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.MaskFormatter;

namespace WB.Tests.Unit.SharedKernels.DataCollection.MaskedFormatterTests
{
    [Subject(typeof(MaskedFormatter))]
    class when_FormatValue_called_for_text_with_only_one_invalid_symbol
    {
        Establish context = () =>
        {
            maskedFormatter = new MaskedFormatter(mask);
        };

        Because of = () =>
            result = maskedFormatter.FormatValue(value, ref cursorPosition);

        It should_result_be_equal_to_emty_mask = () =>
            result.ShouldEqual("__-___-____");

        It should_cursor_be_equal_to_0 = () =>
            cursorPosition.ShouldEqual(0);

        private static MaskedFormatter maskedFormatter;
        private static string result;
        private static string mask = "~*-###-~###";
        private static string value = "1";
        private static int cursorPosition = 1;
    }
}
