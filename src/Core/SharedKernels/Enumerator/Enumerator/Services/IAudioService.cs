using System;
using System.IO;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public enum AudioExceptionType
    {
        Unhandled,
        Io
    }

    public class AudioException : Exception
    {
        public AudioExceptionType Type { get; }
        public AudioException(AudioExceptionType type, string message, Exception innerException) : base(message, innerException)
        {
            this.Type = type;
        }
    }

    public interface IAudioService : IDisposable
    {
        void StartRecording();
        void StopRecording();
        void StartAuditRecording(string fileNamePrefix);
        void StopAuditRecording();
        bool IsAnswerRecording();
        Stream GetRecord(string fileName = null);
        TimeSpan GetLastRecordDuration();
        TimeSpan GetAudioRecordDuration();
        string GetMimeType();
        string GetAudioType();
        double GetNoiseLevel();
        NoiseType GetNoiseType(double noiseLevel);
        event EventHandler OnMaxDurationReached;

        event EventHandler<PlaybackCompletedEventArgs> OnPlaybackCompleted;
        void Play(byte[] content, Identity identity);
        void Stop();

        string GetAuditPath();
    }
    
    public class PlaybackCompletedEventArgs : EventArgs
    {
        public PlaybackCompletedEventArgs(Identity questionIdentity)
        {
            QuestionIdentity = questionIdentity;
        }

        public Identity QuestionIdentity { get; }
    }
}
