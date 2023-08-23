using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard
{
    public abstract class InterviewTabPanel : BaseViewModel
    {
        public abstract DashboardGroupType DashboardType { get; }

        private string title;
        public string Title
        {
            get => this.title;
            set => this.RaiseAndSetIfChanged( ref this.title, value);
        }
    }
}
