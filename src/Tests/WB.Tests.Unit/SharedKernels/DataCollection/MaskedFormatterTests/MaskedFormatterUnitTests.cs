using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.MaskFormatter;

namespace WB.Tests.Unit.SharedKernels.DataCollection.MaskedFormatterTests
{
    [TestFixture]
    public class MaskedFormatterUnitTests
    {
        [TestCase("***-**", "1dr-99", true)]
        [TestCase("#~~-##", "1dr-99", true)]
        [TestCase("***-**", "99", false)]
        [TestCase("***-**", "123456", false)]
        [TestCase("12*456", "123456", true)]
        [TestCase("12*456", "12a456", true)]
        [TestCase("12*456", "12п456", true)]
        [TestCase("12#456", "12п456", false)]
        [TestCase("a*-###-~###", "a9-123-s123", true)]
        public void when_IsTextMaskMatched_called_for_text(string mask, string value, bool isMatched)
        {
            var maskFormater = new MaskedFormatter(mask);

            var isTextMaskMatched = maskFormater.IsTextMaskMatched(value);

            Assert.AreEqual(isTextMaskMatched, isMatched);
        }
    }
}