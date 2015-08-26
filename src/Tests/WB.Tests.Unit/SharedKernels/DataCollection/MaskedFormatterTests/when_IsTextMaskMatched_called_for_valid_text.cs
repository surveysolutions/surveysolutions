using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.MaskFormatter;

namespace WB.Tests.Unit.SharedKernels.DataCollection.MaskedFormatterTests
{
    [Subject(typeof(MaskedFormatter))]
    internal class when_IsTextMaskMatched_called_for_valid_text
    {
        Establish context = () =>
        {
            maskedFormatter = new MaskedFormatter(mask);
        };

        Because of = () =>
            result = maskedFormatter.IsTextMaskMatched(value);

        It should_result_be_true = () =>
            result.ShouldEqual(true);

        private static MaskedFormatter maskedFormatter;
        private static bool result;
        private static string mask = "a*-999-a999";
        private static string value = "a9-123-s123";
    }
}
