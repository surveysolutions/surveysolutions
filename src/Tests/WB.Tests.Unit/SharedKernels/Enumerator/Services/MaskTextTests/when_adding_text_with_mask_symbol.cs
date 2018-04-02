using FluentAssertions;
using NUnit.Framework;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Services.MaskTextTests
{
    internal class when_adding_text_with_mask_symbol : MaskTextTestsContext
    {
        [Test]
        public void should_be_pass_1_to_correct_position()
        {
            int selection = 12;
            var maskedText = CreateMaskedText("*--**");
            //Act
            maskedText.AddString("__1", 0, ref selection);

            maskedText.MakeMaskedText().Should().Be("_--_1");
        }
    }
}
