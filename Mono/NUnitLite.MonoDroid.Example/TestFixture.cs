using Java.Lang;
using NUnit.Framework;

namespace NUnitLite.MonoDroid.Example
{
    [TestFixture]
    public class TestFixture
    {
        [Test]
        public void TestPass()
        {
            Assert.That(true);
        }

        [Test]
        public void TestFail()
        {
            Assert.That(false, "This test failed.");
        }
    
        [Test]
        public void TestException()
        {
            throw new Exception("Test exception");
        }
    }
}