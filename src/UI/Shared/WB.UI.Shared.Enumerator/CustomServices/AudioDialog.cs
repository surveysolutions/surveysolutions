using System;
using System.Threading;
using MvvmCross.Platform.Droid.Platform;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Shared.Enumerator.CustomServices
{
    public class AudioDialog : IAudioDialog
    {
        private AudioDialogFragment dialog;

        private Timer durationTimer;
        private Timer noiseTimer;

        private readonly IMvxAndroidCurrentTopActivity topActivity;
        private readonly IInterviewViewModelFactory viewModelFactory;
        private readonly IAudioService audioService;

        public AudioDialog(IMvxAndroidCurrentTopActivity topActivity, 
            IInterviewViewModelFactory viewModelFactory, 
            IAudioService audioService)
        {
            this.topActivity = topActivity;
            this.viewModelFactory = viewModelFactory;
            this.audioService = audioService;
        }
        
        public event EventHandler OnCanelRecording;
        public event EventHandler OnRecorded;

        public void ShowAndStartRecording(string title)
        {
            if (this.dialog == null)
                this.InitializeDialog();
            
            this.audioService.Start();
            
            this.durationTimer = new Timer(this.OnEvery31Milisecond, null, 0, 31);
            this.noiseTimer = new Timer(this.OnEvery100Millisecond, null, 0, 100);
            
            this.dialog.ViewModel.Title = title;
            this.dialog.ViewModel.NoiseLevel = 0;
            this.dialog.ViewModel.Duration = string.Empty;

            this.dialog.Show(this.topActivity.Activity.FragmentManager, nameof(AudioDialogFragment));
        }

        private void InitializeDialog()
        {
            this.dialog = new AudioDialogFragment
            {
                ViewModel = this.viewModelFactory.GetNew<AudioDialogViewModel>(),
                Cancelable = false
            };
            
            this.dialog.ViewModel.OnCancel += ViewModel_OnCancel;
            this.dialog.ViewModel.OnDone += ViewModel_OnDone;
        }

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

            this.durationTimer.Dispose();
            this.durationTimer = null;

            this.noiseTimer.Dispose();
            this.noiseTimer = null;
            
            this.dialog.Dismiss();
        }

        private void OnEvery31Milisecond(object state)
        {
            var duration = this.audioService.GetDuration();
            this.dialog.ViewModel.Duration = $"{duration.Minutes:00}:{duration.Seconds:00}:{duration.Milliseconds/10:00}";
        }

        private void OnEvery100Millisecond(object state)
        {
            this.dialog.ViewModel.NoiseLevel = this.audioService.GetNoiseLevel();
            this.dialog.ViewModel.NoiseType = this.audioService.GetNoiseType(this.dialog.ViewModel.NoiseLevel);
        }
    }
}