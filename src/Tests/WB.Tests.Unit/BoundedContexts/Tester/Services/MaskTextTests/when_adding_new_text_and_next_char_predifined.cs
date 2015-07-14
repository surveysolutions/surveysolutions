using Machine.Specifications;
using WB.Core.BoundedContexts.Tester.Services.MaskText;
using It = Machine.Specifications.It;


namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.Services.MaskTextTests
{
    internal class when_adding_new_text_and_next_char_predifined : MaskTextTestsContext
    {
        Establish context = () =>
        {
            maskedText = CreateMaskedText("*--**");
        };

        Because of = () =>
            maskedText.AddString("a", 0, ref selection);

        It should_be_accept_char = () =>
            maskedText.MakeMaskedText().ShouldEqual("a--__");

        It should_set_selection_to_3 = () =>
            selection.ShouldEqual(3);


        static int selection;
        static MaskedText maskedText;
    }
}