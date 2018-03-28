using System;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.Core;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class InterviewStageViewModel : MvxViewModel, IDisposable
    {
        public InterviewStageViewModel(MvxViewModel stage, NavigationDirection direction)
        {
            this.Stage = stage;
            this.Direction = direction;
        }

        public MvxViewModel Stage { get; }
        public NavigationDirection Direction { get; }
        public void Dispose() => this.Stage.DisposeIfDisposable();
    }
}