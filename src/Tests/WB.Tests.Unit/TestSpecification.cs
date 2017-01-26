using NUnit.Framework;

namespace WB.Tests.Unit
{
    [TestFixture]
    public class TestSpecification
    {
        [OneTimeSetUp]
        public virtual void Arrange()
        {
            this.Establish();
            this.Because();
        }

        [OneTimeTearDown]
        public virtual void TearDown()
        {
        }

        protected virtual void Establish()
        {
        }

        protected virtual void Because()
        {
        }
    }
}