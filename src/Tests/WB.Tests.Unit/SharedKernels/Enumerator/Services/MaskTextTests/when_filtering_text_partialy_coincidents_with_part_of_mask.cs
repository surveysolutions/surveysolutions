using Machine.Specifications;
using WB.Core.SharedKernels.Enumerator.Services.MaskText;
using It = Machine.Specifications.It;


namespace WB.Tests.Unit.SharedKernels.Enumerator.Services.MaskTextTests
{
    internal class when_filtering_text_partialy_coincidents_with_part_of_mask : MaskTextTestsContext
    {
        Establish context = () =>
        {
            maskedText = CreateMaskedText("*-**");
        };

        Because of = () =>
            filterResult = maskedText.FilterOnlyMaskedChars("ss", 2);

        It should_be_accept_all_chars = () =>
            filterResult.ShouldEqual("ss");

        static string filterResult;
        static MaskedText maskedText;
    }
}