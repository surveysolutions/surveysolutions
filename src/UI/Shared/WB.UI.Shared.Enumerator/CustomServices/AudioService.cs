using System;
using System.Diagnostics;
using Android.Media;
using Android.Webkit;
using Java.IO;
using Java.Lang;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Services;
using Exception = System.Exception;
using Math = System.Math;
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
        private Stopwatch duration = new Stopwatch();

        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly string pathToAudioFile;

        public AudioService(string pathToAudioDirectory, IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.pathToAudioFile = this.fileSystemAccessor.CombinePath(pathToAudioDirectory, audioFileName);
        }

        public void Start()
        {
            if (this.recorder != null) return;

            this.recorder = new MediaRecorder();

            if (this.fileSystemAccessor.IsFileExists(this.pathToAudioFile))
                this.fileSystemAccessor.DeleteFile(this.pathToAudioFile);
            
            this.recorder.SetAudioSource(AudioSource.Mic);
            this.recorder.SetOutputFormat(OutputFormat.Mpeg4);
            this.recorder.SetAudioEncoder(AudioEncoder.Aac);
            this.recorder.SetAudioChannels(1);
            this.recorder.SetAudioSamplingRate(44100);
            this.recorder.SetAudioEncodingBitRate(64000);
            this.recorder.SetOutputFile(this.pathToAudioFile);

            try
            {
                this.recorder.Prepare();
                this.recorder.Start();
                this.duration.Restart();
            }
            catch (Exception ex) when (ex.GetSelfOrInnerAs<IOException>() != null)
            {
                this.ReleaseAudioRecorder();
                throw new AudioException(AudioExceptionType.Io, "Could not write audio file", ex);
            }
            catch (Exception ex) when (ex.GetSelfOrInnerAs<IllegalStateException>() != null)
            {
                this.ReleaseAudioRecorder();
                throw new AudioException(AudioExceptionType.Unhandled, "Unexpected exception during start audio recording", ex);
            }
        }

        public void Stop()
        {
            if (!this.IsRecording()) return;

            this.recorder.Stop();
            this.duration.Stop();
            this.ReleaseAudioRecorder();
        }

        public bool IsRecording() => this.recorder != null;

        public Stream GetLastRecord()
            => this.fileSystemAccessor.IsFileExists(this.pathToAudioFile)
                ? this.fileSystemAccessor.ReadFile(this.pathToAudioFile)
                : null;

        public TimeSpan GetLastRecordDuration() => this.duration.Elapsed;

        public string GetMimeType() => MimeTypeMap.Singleton.GetMimeTypeFromExtension(AudioFileExtension);
        public string GetAudioType() => AudioFileExtension;

        public double GetNoiseLevel()
        {
            if (!this.IsRecording()) return 0;
            return MaxReportableDb + 20 * Math.Log10(this.recorder.MaxAmplitude / MaxReportableAmp);
        }

        public NoiseType GetNoiseType(double noiseLevel)
        {
            if(noiseLevel < 45)
                return NoiseType.Low;
            if(noiseLevel > 80)
                return NoiseType.High;
            return NoiseType.Normal;
        }

        private void ReleaseAudioRecorder()
        {
            this.recorder?.Release();
            this.recorder = null;
        }

        public void Dispose() => this.ReleaseAudioRecorder();
    }
}
