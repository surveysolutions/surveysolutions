using Machine.Specifications;
using WB.Core.BoundedContexts.QuestionnaireTester.Services.MaskText;
using It = Machine.Specifications.It;


namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.Services.MaskTextTests
{
    internal class when_making_text_after_setting_mask : MaskTextTestsContext
    {
        Establish context = () =>
        {
            maskedText = CreateMaskedText("*-##--~~[]");
        };

        Because of = () =>
            result = maskedText.MakeMaskedText();

        It should_be_make_mask_with_correct_char_indexes = () =>
            result.ShouldEqual("_-__--__[]");


        static string result;
        static MaskedText maskedText;
    }
}