using System;
using System.Threading;
using Android.App;
using MvvmCross.Platform.Core;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.UI.Shared.Enumerator.Utils;

namespace WB.UI.Shared.Enumerator.CustomServices
{
    public class AudioDialog : IAudioDialog
    {
        private Dialog dialog;
        private AudioDialogViewModel viewModel;

        private Timer durationTimer;
        private Timer noiseTimer;
        
        private readonly IInterviewViewModelFactory viewModelFactory;
        private readonly IAudioService audioService;

        public AudioDialog(
            IInterviewViewModelFactory viewModelFactory, 
            IAudioService audioService)
        {
            this.viewModelFactory = viewModelFactory;
            this.audioService = audioService;
        }
        
        public event EventHandler OnCanelRecording;
        public event EventHandler OnRecorded;

        public void ShowAndStartRecording(string title)
        {
            if (this.dialog != null)
                throw new Exception("Audio dialog already showed");

            this.dialog = this.ShowDialog();

            this.audioService.OnMaxDurationReached += AudioService_OnMaxDurationReached;
            this.audioService.Start();

            this.durationTimer = new Timer(this.OnEvery31Milisecond, null, 0, 31);
            this.noiseTimer = new Timer(this.OnEvery100Millisecond, null, 0, 100);
        }

        private Dialog ShowDialog()
        {
            this.viewModel = this.viewModelFactory.GetNew<AudioDialogViewModel>();
            this.viewModel.OnCancel += ViewModel_OnCancel;
            this.viewModel.OnDone += ViewModel_OnDone;

            return DialogHelper.ShowDialog(Resource.Layout.interview_question_audio_dialog, viewModel);
        }

        public void StopRecordingAndSaveResult()
        {
            if (this.audioService.IsRecording())
                this.ViewModel_OnDone(this, EventArgs.Empty);
        }

        private void AudioService_OnMaxDurationReached(object sender, EventArgs e)
            => this.StopRecordingAndSaveResult();

        private void ViewModel_OnDone(object sender, EventArgs e)
        {
            this.HideAndStopRecording();
            this.OnRecorded?.Invoke(sender, EventArgs.Empty);
        }
        private void ViewModel_OnCancel(object sender, EventArgs e)
        {
            this.HideAndStopRecording();
            this.OnCanelRecording?.Invoke(sender, e);
        }

        private void HideAndStopRecording()
        {
            this.audioService.Stop();
            this.audioService.OnMaxDurationReached -= this.AudioService_OnMaxDurationReached;

            this.durationTimer.Dispose();
            this.durationTimer = null;

            this.noiseTimer.Dispose();
            this.noiseTimer = null;

            this.dialog.Dismiss();
            this.dialog.Dispose();
            this.dialog = null;

            this.viewModel.OnDone -= this.ViewModel_OnDone;
            this.viewModel.OnCancel -= this.ViewModel_OnCancel;
            this.viewModel.DisposeIfDisposable();
            this.viewModel = null;
        }

        private void OnEvery31Milisecond(object state)
        {
            if(this.viewModel == null) return;
            
            var duration = this.audioService.GetLastRecordDuration();
            this.viewModel.Duration = $"{duration.Minutes:00}:{duration.Seconds:00}:{duration.Milliseconds/10:00}";
        }

        private void OnEvery100Millisecond(object state)
        {
            if (this.viewModel == null) return;

            this.viewModel.NoiseLevel = this.audioService.GetNoiseLevel();
            this.viewModel.NoiseType = this.audioService.GetNoiseType(this.viewModel.NoiseLevel);
        }
    }
}