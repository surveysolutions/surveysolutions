using System;
using System.Threading;
using Android.App;
using Android.Support.V7.App;
using Android.Views;
using MvvmCross.Base;
using MvvmCross.Platforms.Android;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Views;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.UI.Shared.Enumerator.CustomServices
{
    public class AudioDialog : IAudioDialog
    {
        private Dialog modalDialog;
        private AudioDialogViewModel viewModel;
        private MvxAndroidBindingContext modalDialogBindingContext;
        private Timer durationTimer;
        private Timer noiseTimer;

        private readonly IInterviewViewModelFactory viewModelFactory;
        private readonly IAudioService audioService;
        private readonly IMvxAndroidCurrentTopActivity topActivity;

        public AudioDialog(
            IInterviewViewModelFactory viewModelFactory, 
            IAudioService audioService,
            IMvxAndroidCurrentTopActivity topActivity)
        {
            this.viewModelFactory = viewModelFactory;
            this.audioService = audioService;
            this.topActivity = topActivity;
        }
        
        public event EventHandler OnCanelRecording;
        public event EventHandler OnRecorded;

        public void ShowAndStartRecording(string title)
        {
            if (this.modalDialog != null)
                throw new Exception("Audio dialog already showed");

            this.ShowDialog(title);

            this.audioService.OnMaxDurationReached += AudioService_OnMaxDurationReached;
            this.audioService.StartRecording();

            this.durationTimer = new Timer(this.OnEvery41Milisecond, null, 0, 41);
            this.noiseTimer = new Timer(this.OnEvery100Millisecond, null, 0, 100);
        }
        
        private void ShowDialog(string title)
        {
            this.viewModel = this.viewModelFactory.GetNew<AudioDialogViewModel>();
            this.viewModel.Title = title;
            this.viewModel.OnCancel += ViewModel_OnCancel;
            this.viewModel.OnDone += ViewModel_OnDone;

            var parentActivity = this.topActivity.Activity;

            //keep ref to context not to be collected by GC
            this.modalDialogBindingContext = new MvxAndroidBindingContext(parentActivity,
                new MvxSimpleLayoutInflaterHolder(parentActivity.LayoutInflater), viewModel);

            var view = this.modalDialogBindingContext.BindingInflate(Resource.Layout.interview_question_audio_dialog, null, false);

            this.modalDialog = new AppCompatDialog(parentActivity);
            this.modalDialog.Window.RequestFeature(WindowFeatures.NoTitle);
            this.modalDialog.SetCancelable(false);
            this.modalDialog.SetContentView(view);

            this.modalDialog.Show();
            this.modalDialog.Window.SetLayout(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            this.modalDialog.Window.SetBackgroundDrawableResource(Android.Resource.Color.Transparent);
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
            //unsubscribe first before stopping service
            this.durationTimer.Dispose();
            this.durationTimer = null;

            this.noiseTimer.Dispose();
            this.noiseTimer = null;
            
            this.audioService.StopRecording();
            this.audioService.OnMaxDurationReached -= this.AudioService_OnMaxDurationReached;

            this.modalDialog.Dismiss();
            this.modalDialog.Dispose();
            this.modalDialog = null;

            this.viewModel.OnDone -= this.ViewModel_OnDone;
            this.viewModel.OnCancel -= this.ViewModel_OnCancel;
            this.viewModel.DisposeIfDisposable();
            this.viewModel = null;
        }

        private void OnEvery41Milisecond(object state)
        {
            if (this.viewModel == null)
                return;

            var duration = this.audioService.GetLastRecordDuration();
            this.viewModel.Duration = $"{duration.Minutes:00}:{duration.Seconds:00}:{duration.Milliseconds/10:00}";
        }

        private void OnEvery100Millisecond(object state)
        {
            if (this.viewModel == null)
                return;

            this.viewModel.NoiseLevel = this.audioService.GetNoiseLevel();
            this.viewModel.NoiseType = this.audioService.GetNoiseType(this.viewModel.NoiseLevel);
        }
    }
}
