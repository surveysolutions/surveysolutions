using NUnit.Framework;
using WB.Core.GenericSubdomains.Utils;

namespace WB.Tests.Unit.GenericSubdomains.Utils
{
    [TestFixture]
    public class StringExtensionsTests
    {
        [Test]
        public void WhenCalledForMultiwordString()
        {
            var somepascalcased = "SomePascalCased";

            var camelCase = somepascalcased.ToCamelCase();

            Assert.That(camelCase, Is.EqualTo("somePascalCased"));
        }
    }
}