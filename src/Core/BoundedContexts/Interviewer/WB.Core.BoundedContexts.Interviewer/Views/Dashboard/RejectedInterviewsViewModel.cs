using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class RejectedInterviewsViewModel : ListViewModel<IDashboardItem>
    {
        public override GroupStatus InterviewStatus => GroupStatus.StartedInvalid;

        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IInterviewViewModelFactory viewModelFactory;
        private readonly IPrincipal principal;

        public RejectedInterviewsViewModel(
            IPlainStorage<InterviewView> interviewViewRepository,
            IInterviewViewModelFactory viewModelFactory,
            IPrincipal principal)
        {
            this.interviewViewRepository = interviewViewRepository;
            this.viewModelFactory = viewModelFactory;
            this.principal = principal;
        }

        public async Task LoadAsync()
        {
            var rejectedInterviews = await Task.Run(() => this.GetRejectedInterviews().ToList());

            this.Title = string.Format(InterviewerUIResources.Dashboard_RejectedLinkText, rejectedInterviews.Count);

            var subTitle = this.viewModelFactory.GetNew<DashboardSubTitleViewModel>();
            subTitle.Title = InterviewerUIResources.Dashboard_RejectedTabText;
            var uiItems = subTitle.ToEnumerable().Concat(rejectedInterviews).ToList();

            this.Items = rejectedInterviews;
            this.UiItems = new MvxObservableCollection<IDashboardItem>(uiItems);
        }

        private IEnumerable<IDashboardItem> GetRejectedInterviews()
        {
            var interviewerId = this.principal.CurrentUserIdentity.UserId;
            
            var interviewViews = this.interviewViewRepository.Where(interview =>
                interview.ResponsibleId == interviewerId &&
                interview.Status == SharedKernels.DataCollection.ValueObjects.Interview.InterviewStatus.RejectedBySupervisor);

            foreach (var interviewView in interviewViews)
            {
                var interviewDashboardItem = this.viewModelFactory.GetNew<InterviewDashboardItemViewModel>();
                interviewDashboardItem.Init(interviewView);

                yield return interviewDashboardItem;
            }
        }
    }
}