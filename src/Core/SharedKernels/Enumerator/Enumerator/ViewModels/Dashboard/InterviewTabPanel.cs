using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard
{
    public abstract class InterviewTabPanel : MvxViewModel
    {
        public abstract GroupStatus InterviewStatus { get; }

        private string title;
        public string Title
        {
            get => this.title;
            set => MvxNotifyPropertyChangedExtensions.RaiseAndSetIfChanged(this, ref this.title, value);
        }
    }
}
