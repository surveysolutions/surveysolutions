using System;
using System.IO;
using System.Threading;
using Android.Media.Audiofx;
using Android.Views;
using MvvmCross.Platform.Droid.Platform;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Shared.Enumerator.CustomServices
{
    public class AudioDialog : IAudioDialog
    {
        private class VisualizerCapturer : Java.Lang.Object, Visualizer.IOnDataCaptureListener
        {
            public event EventHandler<float> OnWaveCaptured;
            public void OnFftDataCapture(Visualizer visualizer, byte[] fft, int samplingRate)
            {
            }

            public void OnWaveFormDataCapture(Visualizer visualizer, byte[] waveform, int samplingRate) 
                => this.OnWaveCaptured?.Invoke(this, (waveform[0] + 128f) / 256);
        }

        private AudioDialogFragment dialog;

        private Visualizer audioOutput;
        private VisualizerCapturer visualizerCapturer;

        private Timer durationTimer;

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
        public event EventHandler<Stream> OnRecorded;

        public void ShowAndStartRecording(string title, int bitRate)
        {
            if (this.dialog == null)
                this.InitializeDialog();
            
            this.audioService.Start(bitRate);

            this.audioOutput.SetEnabled(true);
            this.durationTimer = new Timer(this.OnEvery100Milisecond, null, 0, 100);
            
            this.dialog.ViewModel.Title = title;
            this.dialog.ViewModel.Intensity = 0;
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

            this.visualizerCapturer = new VisualizerCapturer();
            this.visualizerCapturer.OnWaveCaptured += this.VisualizerCapturer_OnWaveCaptured;

            this.audioOutput = new Visualizer(0); // get output audio stream
            this.audioOutput.SetDataCaptureListener(this.visualizerCapturer, Visualizer.MaxCaptureRate, true, false);
        }

        private void ViewModel_OnDone(object sender, EventArgs e)
        {
            this.HideAndStopRecording();
            this.OnRecorded?.Invoke(sender, this.audioService.GetLastRecord());
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

            this.audioOutput.SetEnabled(false);
            
            this.dialog.Dismiss();
        }

        private void OnEvery100Milisecond(object state)
        {
            var duration = this.audioService.GetDuration();
            this.dialog.ViewModel.Duration = $"{duration.Minutes:00}:{duration.Seconds:00}:{duration.Milliseconds:000}";
        }

        private void VisualizerCapturer_OnWaveCaptured(object sender, float intensity)
            => this.dialog.ViewModel.Intensity = intensity;
    }
}