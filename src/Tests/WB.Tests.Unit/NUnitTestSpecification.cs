using NUnit.Framework;

namespace WB.Tests.Unit
{
    [TestFixture]
    public class NUnitTestSpecification
    {
        [OneTimeSetUp]
        public virtual void Arrange()
        {
            this.Context();
            this.Because();
        }

        [OneTimeTearDown]
        public virtual void TearDown()
        {
        }

        protected virtual void Context()
        {
        }

        protected virtual void Because()
        {
        }
    }
}