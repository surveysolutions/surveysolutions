using Machine.Specifications;
using WB.Core.BoundedContexts.QuestionnaireTester.Services.MaskText;
using It = Machine.Specifications.It;


namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.Services.MaskTextTests
{
    internal class when_adding_new_text_to_empty_mask : MaskTextTestsContext
    {
        Establish context = () =>
        {
            maskedText = CreateMaskedText("*-**");
        };

        Because of = () =>
            maskedText.AddString("sss", 0, ref selection);

        It should_be_accept_all_chars = () =>
            maskedText.MakeMaskedText().ShouldEqual("s-ss");

        It should_set_selection_to_4 = () =>
            selection.ShouldEqual(4);

        static int selection;
        static MaskedText maskedText;
    }
}