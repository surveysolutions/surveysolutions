using Machine.Specifications;
using WB.Core.BoundedContexts.QuestionnaireTester.Services.MaskText;
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

        It should_be_accept_char = () =>
            maskedText.MakeMaskedText().ShouldEqual("1--__");


        static int selection;
        static MaskedText maskedText;
    }
}