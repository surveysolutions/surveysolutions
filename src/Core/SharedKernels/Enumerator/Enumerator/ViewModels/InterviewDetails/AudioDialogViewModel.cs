using System;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class AudioDialogViewModel : MvxViewModel
    {
        public ICommand DoneCommand => new MvxCommand(this.Done);
        public ICommand CancelCommand => new MvxCommand(this.Cancel);

        public event EventHandler OnDone;
        public event EventHandler OnCancel;

        private string title;
        public string Title
        {
            get => this.title;
            set => this.RaiseAndSetIfChanged(ref this.title, value);
        }

        private string duration;
        public string Duration
        {
            get => this.duration;
            set => this.RaiseAndSetIfChanged(ref this.duration, value);
        }

        private int magnitude;
        public int Magnitude
        {
            get => this.magnitude;
            set => this.RaiseAndSetIfChanged(ref this.magnitude, value);
        }

        private MagnitudeType magnitudeType;
        public MagnitudeType MagnitudeType
        {
            get => this.magnitudeType;
            set => this.RaiseAndSetIfChanged(ref this.magnitudeType, value);
        }

        private void Cancel() => this.OnCancel?.Invoke(this, EventArgs.Empty);

        private void Done() => this.OnDone?.Invoke(this, EventArgs.Empty);
    }
}