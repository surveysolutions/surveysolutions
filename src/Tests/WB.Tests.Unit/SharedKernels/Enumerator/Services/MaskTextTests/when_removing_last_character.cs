using Machine.Specifications;
using WB.Core.SharedKernels.Enumerator.Services.MaskText;
using It = Machine.Specifications.It;


namespace WB.Tests.Unit.SharedKernels.Enumerator.Services.MaskTextTests
{
    internal class when_removing_last_character : MaskTextTestsContext
    {
        Establish context = () =>
        {
            maskedText = CreateMaskedText("(###) (### ### ##)");
            maskedText.AddString("(111) (111 111 12)", 0, ref selection);
        };

        Because of = () =>
            maskedText.RemoveRange(16, 1, 0, ref selection);

        It should_be_remove_last_char = () =>
            maskedText.MakeMaskedText().ShouldEqual("(111) (111 111 1_)");

        It should_set_selection_to_16 = () =>
            selection.ShouldEqual(16);


        static int selection;
        static MaskedText maskedText;
    }
}