using Machine.Specifications;
using WB.Core.BoundedContexts.Tester.Services.MaskText;
using It = Machine.Specifications.It;


namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.Services.MaskTextTests
{
    internal class when_adding_new_text_to_midle_of_existed_text : MaskTextTestsContext
    {
        Establish context = () =>
        {
            maskedText = CreateMaskedText("*-**");
            maskedText.AddString("sss", 0, ref selection);
        };

        Because of = () =>
            maskedText.AddString("a", 2, ref selection);

        It should_be_replace_char = () =>
            maskedText.MakeMaskedText().ShouldEqual("s-as");

        It should_set_selection_to_3 = () =>
            selection.ShouldEqual(3);


        static int selection;
        static MaskedText maskedText;
    }
}