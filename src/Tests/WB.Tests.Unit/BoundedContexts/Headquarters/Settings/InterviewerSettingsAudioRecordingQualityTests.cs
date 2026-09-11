using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Settings
{
    [TestOf(typeof(InterviewerSettingsExtensions))]
    public class InterviewerSettingsAudioRecordingQualityTests
    {
        [Test]
        public void when_settings_null_should_return_default_quality()
        {
            InterviewerSettings settings = null;

            Assert.That(settings.GetAudioRecordingQuality(), Is.EqualTo(InterviewerSettings.AudioRecordingQualityDefault));
            Assert.That(InterviewerSettings.AudioRecordingQualityDefault, Is.EqualTo(AudioRecordingQuality.Mono44kHz));
        }

        [Test]
        public void when_quality_not_set_should_return_default_quality()
        {
            var settings = new InterviewerSettings { AudioRecordingQuality = null };

            Assert.That(settings.GetAudioRecordingQuality(), Is.EqualTo(AudioRecordingQuality.Mono44kHz));
        }

        [Test]
        public void when_quality_set_should_return_stored_value()
        {
            var settings = new InterviewerSettings { AudioRecordingQuality = AudioRecordingQuality.Stereo48kHz };

            Assert.That(settings.GetAudioRecordingQuality(), Is.EqualTo(AudioRecordingQuality.Stereo48kHz));
        }
    }
}
