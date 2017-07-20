using System;
using System.Threading.Tasks;
using Android.Media;
using Plugin.Permissions.Abstractions;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.UI.Shared.Enumerator.Services;
using Stream = System.IO.Stream;

namespace WB.UI.Shared.Enumerator.CustomServices
{
    public class AudioService : IAudioService
    {
        private MediaRecorder recorder;
        private DateTime startedDate;
        
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly string pathToAudioFile;

        public AudioService(string pathToAudioDirectory, IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.pathToAudioFile = this.fileSystemAccessor.CombinePath(pathToAudioDirectory, "audio.3gpp");
        }

        public void Start(int bitRate)
        {
            if (this.fileSystemAccessor.IsFileExists(this.pathToAudioFile))
                this.fileSystemAccessor.ReadFile(this.pathToAudioFile);

            if (this.recorder == null)
                this.recorder = new MediaRecorder();

            this.recorder.SetAudioSource(AudioSource.Mic);
            this.recorder.SetOutputFormat(OutputFormat.ThreeGpp);
            this.recorder.SetAudioEncoder(AudioEncoder.AmrNb);
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

        public void Dispose() => this.recorder?.Release();
    }
}
