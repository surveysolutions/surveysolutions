using System;
using MvvmCross.Base;
using MvvmCross.ViewModels;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class InterviewStageViewModel : BaseViewModel
    {
        public InterviewStageViewModel(BaseViewModel stage, NavigationDirection direction)
        {
            this.Stage = stage;
            this.Direction = direction;
        }

        public MvxViewModel Stage { get; }
        public NavigationDirection Direction { get; }
        
        public override void ViewDestroy(bool viewFinishing = true)
        {
            //this.Stage.DisposeIfDisposable();
            base.ViewDestroy(viewFinishing);
        }
 
    }
}
