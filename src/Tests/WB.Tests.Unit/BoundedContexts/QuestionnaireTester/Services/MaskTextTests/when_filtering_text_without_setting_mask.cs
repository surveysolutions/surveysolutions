using Machine.Specifications;
using WB.Core.BoundedContexts.QuestionnaireTester.Services.MaskText;
using It = Machine.Specifications.It;


namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.Services.MaskTextTests
{
    internal class when_filtering_text_without_setting_mask : MaskTextTestsContext
    {
        Establish context = () =>
        {
            maskedText = CreateMaskedText();
        };

        Because of = () =>
            filterResult = maskedText.FilterInserting("s-ss", 0);

        It should_accept_all = () =>
            filterResult.ShouldBeNull();

        static string filterResult;
        static MaskedText maskedText;
    }
}