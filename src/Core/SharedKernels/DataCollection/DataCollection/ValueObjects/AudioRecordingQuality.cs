namespace WB.Core.SharedKernels.DataCollection.ValueObjects
{
    /// <summary>
    /// Audio recording quality used by the interviewer tablet when recording audio.
    /// The default value (0) matches the values previously hard-coded in the AudioService
    /// (mono, 44100 Hz) so that tablets syncing with an older server keep the current behaviour.
    /// </summary>
    public enum AudioRecordingQuality
    {
        Mono44kHz = 0,
        Mono22kHz = 1,
        Stereo44kHz = 2,
        Stereo48kHz = 3,
    }

    public static class AudioRecordingQualityExtensions
    {
        public const int DefaultAudioChannels = 1;
        public const int DefaultSamplingRate = 44100;
        public const int DefaultBitRate = 64000;

        public static int GetAudioChannels(this AudioRecordingQuality quality)
        {
            switch (quality)
            {
                case AudioRecordingQuality.Stereo44kHz:
                case AudioRecordingQuality.Stereo48kHz:
                    return 2;
                default:
                    return 1;
            }
        }

        public static int GetSamplingRate(this AudioRecordingQuality quality)
        {
            switch (quality)
            {
                case AudioRecordingQuality.Mono22kHz:
                    return 22050;
                case AudioRecordingQuality.Stereo48kHz:
                    return 48000;
                default:
                    return 44100;
            }
        }

        public static int GetEncodingBitRate(this AudioRecordingQuality quality)
        {
            return DefaultBitRate * quality.GetAudioChannels();
        }
    }
}
