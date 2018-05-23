using System;
using MvvmCross.Base;
using MvvmCross.ViewModels;

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
