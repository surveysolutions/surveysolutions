using System;
using System.IO;

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
        void Start();
        void Stop();
        bool IsRecording();
        Stream GetLastRecord();
        TimeSpan GetDuration();
        string GetMimeType();
        string GetAudioType();
        double GetNoiseLevel();
        NoiseType GetNoiseType(double noiseLevel);
    }
}
