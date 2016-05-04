using Machine.Specifications;
using WB.Core.SharedKernels.Enumerator.Services.MaskText;
using It = Machine.Specifications.It;


namespace WB.Tests.Unit.SharedKernels.Enumerator.Services.MaskTextTests
{
    internal class when_filtering_text_coincidents_with_mask : MaskTextTestsContext
    {
        Establish context = () =>
        {
            maskedText = CreateMaskedText("*-**");
        };

        Because of = () =>
            filterResult = maskedText.Filter("s-ss", 0);

        It should_be_accept_only_AnyChars = () =>
            filterResult.ShouldEqual("s-ss");

        static string filterResult;
        static MaskedText maskedText;
    }
}