using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.GenericSubdomains.Utils
{
    [TestFixture]
    public class StringExtensionsTests
    {
        [Test]
        public void WhenCalledForMultiwordString()
        {
            Assert.That("SomePascalCased".ToCamelCase(), Is.EqualTo("somePascalCased"));
        }

        [Test]
        public void When_PascalCasing_CamelCased()
        {
            Assert.That("someCamelCase".ToPascalCase(), Is.EqualTo("SomeCamelCase"));
        }
    }
}