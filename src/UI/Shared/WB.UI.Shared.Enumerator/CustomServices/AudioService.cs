﻿using System;
using System.Diagnostics;
using System.IO;
using Android.Media;
using Android.Runtime;
using Android.Webkit;
using Java.Lang;
using MvvmCross.Logging;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
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
        private readonly IMvxLog mvxLog;

        private const int MaxDuration = 3 * 60 * 1000;
        private const double MaxReportableAmp = 32767f;
        private const double MaxReportableDb = 90.3087f;
        private readonly string audioFileName = $"audio.{AudioFileExtension}";
        private const string AudioFileExtension = "m4a";

        private string auditFilePrefix;
        private readonly string pathToAudioAuditDirectory;

        private readonly Stopwatch duration = new Stopwatch();
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly string pathToAudioFile;

        private MediaRecorder recorder;
        private AudioRecorderInfoListener audioRecorderInfoListener;

        private readonly string tempFileName;
        private Identity playingIdentity;
        readonly MediaPlayer mediaPlayer = new MediaPlayer();

        private bool isAuditShouldBeRestarted = false;
        private bool isAuditRecording = false;

        public AudioService(string pathToAudioDirectory, 
            IFileSystemAccessor fileSystemAccessor,
            IAudioFileStorage audioFileStorage, 
            IMvxLogProvider mvxLogProvider)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.audioFileStorage = audioFileStorage;
            this.mvxLog = mvxLogProvider.GetLogFor<AudioService>();
            this.pathToAudioFile = this.fileSystemAccessor.CombinePath(pathToAudioDirectory, audioFileName);
            this.tempFileName = Path.GetTempFileName();
            mediaPlayer.Completion += MediaPlayerOnCompletion;

            this.pathToAudioAuditDirectory = this.fileSystemAccessor.CombinePath(pathToAudioDirectory, "audit");

            if (!this.fileSystemAccessor.IsDirectoryExists(pathToAudioAuditDirectory))
                this.fileSystemAccessor.CreateDirectory(pathToAudioAuditDirectory);
        }

        private void MediaPlayerOnCompletion(object sender, EventArgs eventArgs)
        {
            this.OnOnPlaybackCompleted(new PlaybackCompletedEventArgs(this.playingIdentity));
        }

        public event EventHandler OnMaxDurationReached;
        public event EventHandler<PlaybackCompletedEventArgs> OnPlaybackCompleted;

        public void Play(Guid interviewId, Identity questionId, string fileName)
        {
            lock (this.lockObject)
            {
                if (this.mediaPlayer.IsPlaying)
                {
                    this.mediaPlayer.Stop();
                    OnOnPlaybackCompleted(new PlaybackCompletedEventArgs(this.playingIdentity));
                }
                
                this.mediaPlayer.Reset();
                this.fileSystemAccessor.DeleteFile(this.tempFileName);

                var interviewBinaryData = this.audioFileStorage.GetInterviewBinaryData(interviewId, fileName);
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
            return pathToAudioAuditDirectory;
        }

        public void StartRecording()
        {
            if (isAuditRecording && this.recorder != null)
                StopRecordingInt();

            if (this.recorder != null)
                return;

            if (this.fileSystemAccessor.IsFileExists(this.pathToAudioFile))
                this.fileSystemAccessor.DeleteFile(this.pathToAudioFile);

            isAuditRecording = false;
            this.duration.Restart();
            Record(this.pathToAudioFile, MaxDuration);
        }

        public void StartAuditRecording(string fileNamePrefix)
        {
            isAuditShouldBeRestarted = true;

            this.auditFilePrefix = fileNamePrefix;
            var fileNameWithExtension = $"{this.auditFilePrefix}-{DateTime.Now:yyyyMMdd_HHmmssfff}.{AudioFileExtension}";

            var fullPath = this.fileSystemAccessor.CombinePath(pathToAudioAuditDirectory, fileNameWithExtension);

            isAuditRecording = true;
            Record(fullPath, int.MaxValue);
        }

        private void Record(string audioFilePath, int maxDuration)
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
            this.audioRecorderInfoListener.OnMaxDurationReached += this.AudioRecorderInfoListenerOnMaxDurationReached;
            this.recorder.SetOnInfoListener(audioRecorderInfoListener);

            try
            {
                this.recorder.Prepare();
                this.recorder.Start();
                this.mvxLog.Debug("Started Audio audit recording");
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

            this.recorder.Stop();
            this.duration.Stop();
            this.ReleaseAudioRecorder();
            this.mvxLog.Debug("Stopped Audio audit recording");
        }

        public void StopAuditRecording()
        {
            StopRecordingInt();
        }

        public bool IsAnswerRecording() => (this.recorder != null && !isAuditRecording);

        public Stream GetRecord(string fileName = null)
            => this.fileSystemAccessor.IsFileExists(fileName ?? this.pathToAudioFile)
                ? this.fileSystemAccessor.ReadFile(fileName ?? this.pathToAudioFile)
                : null;

        public TimeSpan GetLastRecordDuration() => this.duration.Elapsed;

        public TimeSpan GetAudioRecordDuration()
        {
            if(!this.fileSystemAccessor.IsFileExists(this.pathToAudioFile))
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
        public event EventHandler OnMaxDurationReached;
        public void OnInfo(MediaRecorder mr, MediaRecorderInfo what, int extra)
        {
            if(what == MediaRecorderInfo.MaxDurationReached)
                this.OnMaxDurationReached?.Invoke(this, EventArgs.Empty);
        }
    }
}
