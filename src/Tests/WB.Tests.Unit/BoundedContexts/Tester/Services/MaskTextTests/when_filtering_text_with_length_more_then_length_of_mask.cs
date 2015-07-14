using Machine.Specifications;
using WB.Core.BoundedContexts.Tester.Services.MaskText;
using It = Machine.Specifications.It;


namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.Services.MaskTextTests
{
    internal class when_filtering_text_with_length_more_then_length_of_mask : MaskTextTestsContext
    {
        Establish context = () =>
        {
            maskedText = CreateMaskedText("*-**");
        };

        Because of = () =>
            filterResult = maskedText.FilterOnlyMaskedChars("sssssssssss", 0);

        It should_be_accept_only_3_chars = () =>
            filterResult.ShouldEqual("sss");

        static string filterResult;
        static MaskedText maskedText;
    }
}