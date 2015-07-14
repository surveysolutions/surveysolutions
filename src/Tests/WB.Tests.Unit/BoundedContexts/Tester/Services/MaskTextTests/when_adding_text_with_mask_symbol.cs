using Machine.Specifications;
using WB.Core.BoundedContexts.Tester.Services.MaskText;
using It = Machine.Specifications.It;


namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.Services.MaskTextTests
{
    internal class when_adding_text_with_mask_symbol : MaskTextTestsContext
    {
        Establish context = () =>
        {
            maskedText = CreateMaskedText("*--**");
        };

        Because of = () =>
            maskedText.AddString("__1", 0, ref selection);

        It should_be_pass_1_to_correct_position = () =>
            maskedText.MakeMaskedText().ShouldEqual("_--_1");


        static int selection;
        static MaskedText maskedText;
    }
}