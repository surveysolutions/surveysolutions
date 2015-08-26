using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.MaskFormatter;

namespace WB.Tests.Unit.SharedKernels.DataCollection.MaskedFormatterTests
{
    [Subject(typeof(MaskedFormatter))]
    internal class when_IsTextMaskMatched_called_for_invalid_text
    {
        Establish context = () =>
        {
            maskedFormatter = new MaskedFormatter(mask);
        };

        Because of = () =>
            result = maskedFormatter.IsTextMaskMatched(value);

        It should_result_be_false = () =>
            result.ShouldEqual(false);

        private static MaskedFormatter maskedFormatter;
        private static bool result;
        private static string mask = "@*-###-@###";
        private static string value = "19-123-s123";
    }
}
