using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.GenericSubdomains.Utils
{
    public class DecimalExtensionsTests
    {
        [SetCulture("es-ES")]
        [TestCase(1, "1")]
        [TestCase(11245, "11.245")]
        [TestCase(11111111.45, "11.111.111,45")]
        [TestCase(0.555, "0,555")]
        [TestCase(-0.555, "-0,555")]
        public void decimalFormatting(decimal value, string expectedResult)
        {
            var formattedDecimal = value.FormatDecimal();
            Assert.That(formattedDecimal, Is.EqualTo(expectedResult));
        }
    }
}