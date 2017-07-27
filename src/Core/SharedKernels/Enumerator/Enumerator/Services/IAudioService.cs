using System;
using System.IO;

namespace WB.Core.SharedKernels.Enumerator.Services
{
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
