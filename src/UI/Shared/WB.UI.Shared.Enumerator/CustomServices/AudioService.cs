using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Android.Media;
using Android.Runtime;
using Android.Webkit;
using Java.Lang;
using MvvmCross.Logging;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using WB.UI.Shared.Enumerator.Services;
using Exception = System.Exception;
using IOException = Java.IO.IOException;
using Math = System.Math;
using Stream = System.IO.Stream;

namespace WB.UI.Shared.Enumerator.CustomServices
{
    public class AudioService : IAudioService
    {
        private bool disposed = false;
        private object lockObject = new object();

        private readonly IAudioFileStorage audioFileStorage;
        private readonly ILogger logger;
        private readonly IWorkspaceAccessor workspaceAccessor;

        private const int MaxDuration = 3 * 60 * 1000;
        private const double MaxReportableAmp = 32767f;
        private const double MaxReportableDb = 90.3087f;

        private readonly int audioAuditMaxFileDuration = (int) TimeSpan.FromMinutes(20).TotalMilliseconds;
        private readonly string audioFileName = $"audio.{AudioFileExtension}";
        private const string AudioFileExtension = "m4a";

        private string auditFilePrefix;

        private readonly Stopwatch duration = new Stopwatch();
        private readonly string audioDirectory;
        private readonly IFileSystemAccessor fileSystemAccessor;

        private MediaRecorder recorder;
        private AudioRecorderInfoListener audioRecorderInfoListener;

        private readonly string tempFileName;
        private Identity playingIdentity;
        readonly MediaPlayer mediaPlayer = new MediaPlayer();

        private bool isAuditShouldBeRestarted = false;
        private bool isAuditRecording = false;

        public AudioService(string audioDirectory, 
            IFileSystemAccessor fileSystemAccessor,
            IAudioFileStorage audioFileStorage, 
            ILogger logger,
            IWorkspaceAccessor workspaceAccessor)
        {
            this.audioDirectory = audioDirectory;
            this.fileSystemAccessor = fileSystemAccessor;
            this.audioFileStorage = audioFileStorage;
            this.logger = logger;
            this.workspaceAccessor = workspaceAccessor;
            
            this.tempFileName = Path.GetTempFileName();
            mediaPlayer.Completion += MediaPlayerOnCompletion;
        }

        /*public AudioService(string pathToAudioDirectory, 
            IFileSystemAccessor fileSystemAccessor,
            IAudioFileStorage audioFileStorage, 
            ILogger logger)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.audioFileStorage = audioFileStorage;
            this.logger = logger;
            if (!fileSystemAccessor.IsDirectoryExists(pathToAudioDirectory))
                fileSystemAccessor.CreateDirectory(pathToAudioDirectory);
            this.pathToAudioFile = this.fileSystemAccessor.CombinePath(pathToAudioDirectory, audioFileName);
            this.tempFileName = Path.GetTempFileName();
            mediaPlayer.Completion += MediaPlayerOnCompletion;

            this.pathToAudioAuditDirectory = this.fileSystemAccessor.CombinePath(pathToAudioDirectory, "audit");

            if (!this.fileSystemAccessor.IsDirectoryExists(pathToAudioAuditDirectory))
                this.fileSystemAccessor.CreateDirectory(pathToAudioAuditDirectory);
        }*/

        private string GetPathToAudioFile()
        {
            var pathToAudioDirectory = GetPathToAudioDirectory();
            var pathToFile = this.fileSystemAccessor.CombinePath(pathToAudioDirectory, audioFileName);
            return pathToFile;
        }

        private string GetPathToAudioDirectory()
        {
            var appDirectory = AndroidPathUtils.GetPathToInternalDirectory();
            var workspace = workspaceAccessor.GetCurrentWorkspaceName();

            var pathToAudioDirectory = fileSystemAccessor.CombinePath(
                appDirectory,
                workspace,
                audioDirectory);
            if (!fileSystemAccessor.IsDirectoryExists(pathToAudioDirectory))
                fileSystemAccessor.CreateDirectory(pathToAudioDirectory);
            return pathToAudioDirectory;
        }

        private string GetPathToAudioAuditDirectory()
        {
            var pathToAudioDirectory = GetPathToAudioDirectory();
            var pathToAudioAuditDirectory = this.fileSystemAccessor.CombinePath(pathToAudioDirectory, "audit");

            if (!this.fileSystemAccessor.IsDirectoryExists(pathToAudioAuditDirectory))
                this.fileSystemAccessor.CreateDirectory(pathToAudioAuditDirectory);

            return pathToAudioAuditDirectory;
        }

        private void MediaPlayerOnCompletion(object sender, EventArgs eventArgs)
        {
            this.OnOnPlaybackCompleted(new PlaybackCompletedEventArgs(this.playingIdentity));
        }

        public event EventHandler OnMaxDurationReached;
        public event EventHandler<PlaybackCompletedEventArgs> OnPlaybackCompleted;

        public async Task Play(Guid interviewId, Identity questionId, string fileName)
        {
            var interviewBinaryData = await this.audioFileStorage.GetInterviewBinaryDataAsync(interviewId, fileName);

            lock (this.lockObject)
            {
                if (this.mediaPlayer.IsPlaying)
                {
                    this.mediaPlayer.Stop();
                    OnOnPlaybackCompleted(new PlaybackCompletedEventArgs(this.playingIdentity));
                }
                
                this.mediaPlayer.Reset();
                this.fileSystemAccessor.DeleteFile(this.tempFileName);

                this.fileSystemAccessor.WriteAllBytes(this.tempFileName, interviewBinaryData);
                
                this.mediaPlayer.SetDataSource(this.tempFileName);
                this.mediaPlayer.SetVolume(1, 1);
                this.mediaPlayer.Prepare();
                this.mediaPlayer.Start();
                this.playingIdentity = questionId;
            }
        }

        public void Stop()
        {
            lock (this.lockObject)
            {
                if (this.mediaPlayer.IsPlaying)
                {
                    this.mediaPlayer.Reset();
                    this.fileSystemAccessor.DeleteFile(this.tempFileName);
                }
            }
        }

        public string GetAuditPath()
        {
            return GetPathToAudioDirectory();
        }

        public void StartRecording()
        {
            if (isAuditRecording && this.recorder != null)
                StopRecordingInt();

            if (this.recorder != null)
                return;

            var pathToAudioFile = this.GetPathToAudioFile();
            if (this.fileSystemAccessor.IsFileExists(pathToAudioFile))
                this.fileSystemAccessor.DeleteFile(pathToAudioFile);

            isAuditRecording = false;
            this.duration.Restart();
            Record(pathToAudioFile, MaxDuration);
        }

        public void StartAuditRecording(string fileNamePrefix)
        {
            isAuditShouldBeRestarted = true;

            this.OnMaxDurationReached += AudioAudit_OnMaxDurationReached;

            this.auditFilePrefix = fileNamePrefix;
            RecordAudioAudit();
        }

        private void AudioAudit_OnMaxDurationReached(object sender, EventArgs e)
        {
            RecordAudioAudit();
        }

        private void RecordAudioAudit()
        {
            var fileNameWithExtension = $"{this.auditFilePrefix}-{DateTime.Now:yyyyMMdd_HHmmssfff}.{AudioFileExtension}";

            var pathToAudioAuditDirectory = GetPathToAudioAuditDirectory();
            var fullPath = this.fileSystemAccessor.CombinePath(pathToAudioAuditDirectory, fileNameWithExtension);

            isAuditRecording = true;
            Record(fullPath, audioAuditMaxFileDuration);
        }

        private void Record(string audioFilePath, int maxDuration)
        {
            lock (this.lockObject)
            {
                this.ReleaseAudioRecorder();

                this.recorder = new MediaRecorder();

                this.recorder.SetAudioSource(AudioSource.Mic);
                this.recorder.SetOutputFormat(OutputFormat.Mpeg4);
                this.recorder.SetAudioEncoder(AudioEncoder.Aac);
                this.recorder.SetAudioChannels(1);
                this.recorder.SetAudioSamplingRate(44100);
                this.recorder.SetAudioEncodingBitRate(64000);
                this.recorder.SetOutputFile(audioFilePath);
                this.recorder.SetMaxDuration(maxDuration);

                this.audioRecorderInfoListener = new AudioRecorderInfoListener();
                this.audioRecorderInfoListener.OnMaxDurationReached +=
                    this.AudioRecorderInfoListenerOnMaxDurationReached;
                this.recorder.SetOnInfoListener(audioRecorderInfoListener);

                try
                {
                    this.recorder.Prepare();
                    this.recorder.Start();
                    this.logger.Info($"Started recording to file {audioFilePath}");
                }
                catch (Exception ex) when (ex.GetSelfOrInnerAs<IOException>() != null)
                {
                    this.ReleaseAudioRecorder();
                    throw new AudioException(AudioExceptionType.Io, "Could not write audio file", ex);
                }
                catch (Exception ex) when (ex.GetSelfOrInnerAs<IllegalStateException>() != null)
                {
                    this.ReleaseAudioRecorder();
                    throw new AudioException(AudioExceptionType.Unhandled,
                        "Unexpected exception during start audio recording", ex);
                }
            }
        }

        private void AudioRecorderInfoListenerOnMaxDurationReached(object sender, EventArgs e)
            => this.OnMaxDurationReached?.Invoke(this, EventArgs.Empty);

        public void StopRecording()
        {
            StopRecordingInt();

            if (isAuditShouldBeRestarted)
                StartAuditRecording(auditFilePrefix);
        }


        private void StopRecordingInt()
        {
            if (this.recorder == null)
                return;
            
            lock (this.lockObject)
            {
                try
                {
                    this.recorder.Stop();
                }
                catch (Exception ex) when (ex.GetSelfOrInnerAs<IllegalStateException>() != null)
                {
                    throw new AudioException(AudioExceptionType.Unhandled,
                        "Unexpected exception during stop audio recording", ex);
                }
                finally
                {
                    this.duration.Stop();
                    this.ReleaseAudioRecorder();
                    this.OnMaxDurationReached -= AudioAudit_OnMaxDurationReached;
                    this.logger.Info($"Stopped recording");
                }
            }
        }

        public void StopAuditRecording()
        {
            StopRecordingInt();
        }

        public bool IsAnswerRecording() => (this.recorder != null && !isAuditRecording);

        public Stream GetRecord(string fileName = null)
        {
            fileName ??= GetPathToAudioFile();
            return this.fileSystemAccessor.IsFileExists(fileName)
                ? this.fileSystemAccessor.ReadFile(fileName)
                : null;
        }

        public TimeSpan GetLastRecordDuration() => this.duration.Elapsed;

        public TimeSpan GetAudioRecordDuration()
        {
            var pathToAudioFile = GetPathToAudioFile();
            if(!this.fileSystemAccessor.IsFileExists(pathToAudioFile))
            {
                return TimeSpan.Zero;
            }

            var metaRetriever = new MediaMetadataRetriever();
            metaRetriever.SetDataSource(pathToAudioFile);
            var durationMs = metaRetriever.ExtractMetadata(MetadataKey.Duration);

            return long.TryParse(durationMs, out long duration) 
                ? TimeSpan.FromMilliseconds(duration) 
                : TimeSpan.Zero;
        }

        public string GetMimeType() => MimeTypeMap.Singleton.GetMimeTypeFromExtension(AudioFileExtension);
        public string GetAudioType() => AudioFileExtension;

        public double GetNoiseLevel()
        {
            if (!this.IsAnswerRecording()) return 0;
            return MaxReportableDb + 20 * Math.Log10(this.GetMaxAmplitude() / MaxReportableAmp);
        }

        public NoiseType GetNoiseType(double noiseLevel)
        {
            if(noiseLevel < 45)
                return NoiseType.Low;
            if(noiseLevel > 80)
                return NoiseType.High;
            return NoiseType.Normal;
        }

        private int GetMaxAmplitude()
        {
            var maxAmplitude = 0;
            try
            {
                maxAmplitude = this.recorder.MaxAmplitude;
            }
            catch (RuntimeException)
            {
                /*still don't understand when this exception can be*/
                /* developer.android.com */
                /* int getMaxAmplitude () - Returns the maximum absolute amplitude that was sampled since the last call to this method. Call this only after the setAudioSource(). */
                /* Returns the maximum absolute amplitude measured since the last call, or 0 when called for the first time */
                /* Throws IllegalStateException if it is called before the audio source has been set. */

                /* android.googlesource.com */
                /* 
                static int android_media_MediaRecorder_native_getMaxAmplitude(JNIEnv *env, jobject thiz)
                {
                   ALOGV("getMaxAmplitude");
                   sp<MediaRecorder> mr = getMediaRecorder(env, thiz);
                   int result = 0;
                   process_media_recorder_call(env, mr->getMaxAmplitude(&result), "java/lang/RuntimeException", "getMaxAmplitude failed.");
                   return result;
                }
                */
            }

            return maxAmplitude;
        }

        private void ReleaseAudioRecorder()
        {
            if (this.audioRecorderInfoListener != null)
            {
                this.audioRecorderInfoListener.OnMaxDurationReached -= this.AudioRecorderInfoListenerOnMaxDurationReached;
                this.audioRecorderInfoListener.Dispose();
                this.audioRecorderInfoListener = null;
            }

            this.recorder?.Reset();
            this.recorder?.Release();
            this.recorder = null;
        }

        public void Dispose()
        {
            if (disposed) return;
            
            this.ReleaseAudioRecorder();

            // audio service is a singleton and can be disposed by activity lifecycle
            // this is a temp fix. Either AudioService shouldn't be a singleton, either should not be disposed or affected by elements with short lifecycle       
            //this.mediaPlayer.Dispose();

            this.disposed = true;
        }

        protected virtual void OnOnPlaybackCompleted(PlaybackCompletedEventArgs e)
        {
            OnPlaybackCompleted?.Invoke(this, e);
        }
    }

    public class AudioRecorderInfoListener : Java.Lang.Object, MediaRecorder.IOnInfoListener
    {
        public AudioRecorderInfoListener() { }
        public AudioRecorderInfoListener(System.IntPtr pointer, Android.Runtime.JniHandleOwnership ownership)
            :base(pointer, ownership) { }

        public event EventHandler OnMaxDurationReached;
        public void OnInfo(MediaRecorder mr, MediaRecorderInfo what, int extra)
        {
            if(what == MediaRecorderInfo.MaxDurationReached)
                this.OnMaxDurationReached?.Invoke(this, EventArgs.Empty);
        }
    }
}
