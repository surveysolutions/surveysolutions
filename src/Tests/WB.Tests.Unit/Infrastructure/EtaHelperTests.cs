using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.Infrastructure
{
    public class EtaHelperTests
    {
        [Test]
        public void should_calculate_eta()
        {
            var etahelper = new EtaHelper(1100, 100, averageWindow: 5);

            etahelper.AddProgress(100, 100);
            etahelper.AddProgress(100, 100);
            etahelper.AddProgress(100, 100);
            etahelper.AddProgress(100, 100);
            var eta = etahelper.AddProgress(100, 100);

            Assert.That(eta.TotalMilliseconds, Is.EqualTo(600));

            etahelper.AddProgress(10, 100);
            etahelper.AddProgress(10, 100);
            etahelper.AddProgress(10, 100);
            etahelper.AddProgress(10, 100);
            eta = etahelper.AddProgress(10, 100);

            Assert.That(eta.TotalMilliseconds, Is.EqualTo(10));
        }
    }
}
