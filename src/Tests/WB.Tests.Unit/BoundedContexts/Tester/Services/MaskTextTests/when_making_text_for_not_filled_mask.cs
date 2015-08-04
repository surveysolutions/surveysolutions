using Machine.Specifications;
using WB.Core.BoundedContexts.Tester.Services.MaskText;
using It = Machine.Specifications.It;


namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.Services.MaskTextTests
{
    internal class when_making_text_for_not_filled_mask : MaskTextTestsContext
    {
        Establish context = () =>
        {
            maskedText = CreateMaskedText("*-**");
            maskedText.AddString("_-_s", 0, ref selection);
        };

        Because of = () =>
            result = maskedText.MakeMaskedText();

        It should_be_make_mask_with_correct_char_indexes = () =>
            result.ShouldEqual("_-_s");


        static int selection;
        static string result;
        static MaskedText maskedText;
    }
}