using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.DataCollection.AudioAuditTests
{
    [TestOf(typeof(AudioRecordingQualityExtensions))]
    public class AudioRecordingQualityExtensionsTests
    {
        [Test]
        public void default_quality_should_match_current_audio_service_values()
        {
            var quality = AudioRecordingQuality.Mono44kHz;

            Assert.That((int)quality, Is.EqualTo(0));
            Assert.That(quality.GetAudioChannels(), Is.EqualTo(AudioRecordingQualityExtensions.DefaultAudioChannels));
            Assert.That(quality.GetSamplingRate(), Is.EqualTo(AudioRecordingQualityExtensions.DefaultSamplingRate));
            Assert.That(quality.GetEncodingBitRate(), Is.EqualTo(AudioRecordingQualityExtensions.DefaultBitRate));
        }

        [TestCase(AudioRecordingQuality.Mono44kHz, 1, 44100)]
        [TestCase(AudioRecordingQuality.Mono22kHz, 1, 22050)]
        [TestCase(AudioRecordingQuality.Mono16kHz, 1, 16000)]
        [TestCase(AudioRecordingQuality.Stereo44kHz, 2, 44100)]
        [TestCase(AudioRecordingQuality.Stereo48kHz, 2, 48000)]
        public void should_map_quality_to_channels_and_sampling_rate(AudioRecordingQuality quality, int channels, int samplingRate)
        {
            Assert.That(quality.GetAudioChannels(), Is.EqualTo(channels));
            Assert.That(quality.GetSamplingRate(), Is.EqualTo(samplingRate));
        }

        [Test]
        public void stereo_encoding_bit_rate_should_scale_with_channels()
        {
            Assert.That(AudioRecordingQuality.Stereo44kHz.GetEncodingBitRate(),
                Is.EqualTo(AudioRecordingQualityExtensions.DefaultBitRate * 2));
        }

        [Test]
        public void mono16kHz_encoding_bit_rate_should_be_32kbps()
        {
            Assert.That(AudioRecordingQuality.Mono16kHz.GetEncodingBitRate(), Is.EqualTo(32000));
        }
    }
}
