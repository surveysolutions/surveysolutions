using MvvmCross.Core.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public abstract class InterviewTabPanel : MvxViewModel
    {
        public abstract GroupStatus InterviewStatus { get; }

        private string title;
        public string Title
        {
            get { return this.title; }
            set { this.title = value; this.RaisePropertyChanged(); }
        }
    }
}