using FluentAssertions;
using WB.Core.SharedKernels.Enumerator.Services.MaskText;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Services.MaskTextTests
{
    internal class when_adding_new_text_to_midle_of_existed_text : MaskTextTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            maskedText = CreateMaskedText("*-**");
            maskedText.AddString("sss", 0, ref selection);
            BecauseOf();
        }

        private void BecauseOf() =>
            maskedText.AddString("a", 2, ref selection);

        [NUnit.Framework.Test] public void should_be_replace_char () =>
            maskedText.MakeMaskedText().Should().Be("s-as");

        [NUnit.Framework.Test] public void should_set_selection_to_3 () =>
            selection.Should().Be(3);


        static int selection;
        static MaskedText maskedText;
    }
}
