using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class RejectedInterviewsViewModel : ListViewModel<InterviewDashboardItemViewModel>
    {
        public string Description => InterviewerUIResources.Dashboard_RejectedTabText;
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

        public void Load()
        {
            this.Items = this.GetRejectedInterviews().ToList();
            this.Title = string.Format(InterviewerUIResources.Dashboard_RejectedLinkText, this.Items.Count);
        }

        private IEnumerable<InterviewDashboardItemViewModel> GetRejectedInterviews()
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