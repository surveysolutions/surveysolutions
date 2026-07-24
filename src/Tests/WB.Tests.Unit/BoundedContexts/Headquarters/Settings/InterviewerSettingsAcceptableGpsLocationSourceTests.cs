using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Settings
{
    [TestOf(typeof(InterviewerSettingsExtensions))]
    public class InterviewerSettingsAcceptableGpsLocationSourceTests
    {
        [Test]
        public void when_settings_null_should_return_default_source()
        {
            InterviewerSettings settings = null;

            Assert.That(settings.GetAcceptableGpsLocationSource(), Is.EqualTo(InterviewerSettings.AcceptableGpsLocationSourceDefault));
            Assert.That(InterviewerSettings.AcceptableGpsLocationSourceDefault, Is.EqualTo(AcceptableGpsLocationSource.BuiltInGpsOnly));
        }

        [Test]
        public void when_source_not_set_should_return_default_source()
        {
            var settings = new InterviewerSettings { AcceptableGpsLocationSource = null };

            Assert.That(settings.GetAcceptableGpsLocationSource(), Is.EqualTo(AcceptableGpsLocationSource.BuiltInGpsOnly));
        }

        [Test]
        public void when_source_set_should_return_stored_value()
        {
            var settings = new InterviewerSettings { AcceptableGpsLocationSource = AcceptableGpsLocationSource.Any };

            Assert.That(settings.GetAcceptableGpsLocationSource(), Is.EqualTo(AcceptableGpsLocationSource.Any));
        }
    }
}
