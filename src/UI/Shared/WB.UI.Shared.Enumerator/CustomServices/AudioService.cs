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

        public void Start(int kbPerSec)
        {
            var bitRate = kbPerSec * 1000;

            if (this.fileSystemAccessor.IsFileExists(this.pathToAudioFile))
                this.fileSystemAccessor.ReadFile(this.pathToAudioFile);

            if (this.recorder == null)
                this.recorder = new MediaRecorder();
            
            this.recorder.SetAudioSource(AudioSource.Mic);
            this.recorder.SetOutputFormat(OutputFormat.Mpeg4);
            this.recorder.SetAudioEncoder(AudioEncoder.Aac);
            this.recorder.SetAudioEncodingBitRate(bitRate);
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

        public int GetMagnitude()
        {
            /* 
             * Update the microphone state DB is the relative loudness decibels formula K=20*lg(vo/vi)
             * vo current amplitude value 
             * vi benchmark value of 600
             */

            int vo = this.recorder?.MaxAmplitude ?? 0;
            int vi = 600;
            int ratio = vo / vi;

            int db = 0;
            if (ratio > 1)
                db = (int)(20 * Math.Log10(ratio));

            return db;
        }

        public MagnitudeType GetMagnitudeType()
        {
            var magnitude = this.GetMagnitude();
            if(magnitude < 4)
                return MagnitudeType.Low;
            if(magnitude > 20)
                return MagnitudeType.High;
            return MagnitudeType.Normal;
        }

        public void Dispose() => this.recorder?.Release();
    }
}
