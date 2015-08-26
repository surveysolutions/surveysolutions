using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.MaskFormatter;

namespace WB.Core.BoundedContexts.Capi.Tests.UI.MaskedFormatterTests
{
    [Subject(typeof(MaskedFormatter))]
    internal class when_FormatValue_after_insert_value_longer_than_mask    {
        private Establish context = () =>
        {
            maskedFormatter = new MaskedFormatter(mask);
        };

        private Because of = () =>
            result = maskedFormatter.FormatValue(value, ref cursorPosition);

        private It should_result_be_equal_to_formatted_value = () =>
            result.ShouldEqual("w1-234-b___");

        private static MaskedFormatter maskedFormatter;
        private static string result;
        private static string mask = "~*-###-~###";
        private static string value = "w1-234bablbablablabla-___";
        private static int cursorPosition = 22;
    }
}
