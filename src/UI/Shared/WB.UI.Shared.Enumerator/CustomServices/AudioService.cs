using System;
using Android.Media;
using Android.Webkit;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Services;
using Stream = System.IO.Stream;

namespace WB.UI.Shared.Enumerator.CustomServices
{
    public class AudioService : IAudioService
    {
        private const double MaxReportableAmp = 32767f;
        private const double MaxReportableDb = 90.3087f;
        private readonly string audioFileName = $"audio.{AudioFileExtension}";
        private const string AudioFileExtension = "m4a";
        private MediaRecorder recorder;
        private DateTime startedDate;
        
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly string pathToAudioFile;

        public AudioService(string pathToAudioDirectory, IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.pathToAudioFile = this.fileSystemAccessor.CombinePath(pathToAudioDirectory, audioFileName);
        }

        public void Start()
        {
            if (this.fileSystemAccessor.IsFileExists(this.pathToAudioFile))
                this.fileSystemAccessor.ReadFile(this.pathToAudioFile);

            if (this.recorder == null)
                this.recorder = new MediaRecorder();
            
            this.recorder.SetAudioSource(AudioSource.Mic);
            this.recorder.SetOutputFormat(OutputFormat.Mpeg4);
            this.recorder.SetAudioEncoder(AudioEncoder.Aac);
            this.recorder.SetAudioEncodingBitRate(64000);
            this.recorder.SetOutputFile(this.pathToAudioFile);
            this.recorder.Prepare();
            this.recorder.Start();

            this.startedDate = DateTime.Now;
        }

        public void Stop() => this.recorder?.Stop();

        public Stream GetLastRecord()
            => this.fileSystemAccessor.IsFileExists(this.pathToAudioFile)
                ? this.fileSystemAccessor.ReadFile(this.pathToAudioFile)
                : null;

        public TimeSpan GetDuration() => DateTime.Now - this.startedDate;
        public string GetMimeType() => MimeTypeMap.Singleton.GetMimeTypeFromExtension(audioFileName);
        public string GetAudioType() => AudioFileExtension;

        private double prevNoiseLevel;
        public double GetNoiseLevel()
        {
            double maxAmplitude = this.recorder.MaxAmplitude;
            if (maxAmplitude == 0) return prevNoiseLevel;

            this.prevNoiseLevel = MaxReportableDb + (20 * Math.Log10(maxAmplitude / MaxReportableAmp));

            return prevNoiseLevel;
        }

        public NoiseType GetNoiseType()
        {
            if(this.prevNoiseLevel < 45)
                return NoiseType.Low;
            if(this.prevNoiseLevel > 80)
                return NoiseType.High;
            return NoiseType.Normal;
        }

        public void Dispose() => this.recorder?.Release();
    }
}
