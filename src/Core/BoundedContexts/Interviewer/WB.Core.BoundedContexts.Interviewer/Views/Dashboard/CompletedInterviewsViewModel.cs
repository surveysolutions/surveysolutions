using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class CompletedInterviewsViewModel : BaseInterviewsViewModel
    {
        public override GroupStatus InterviewStatus => GroupStatus.Completed;
        protected override string TabTitle => InterviewerUIResources.Dashboard_CompletedLinkText;
        protected override string TabDescription => InterviewerUIResources.Dashboard_CompletedTabText;

        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IPrincipal principal;

        public event EventHandler<InterviewRemovedArgs> OnInterviewRemoved;

        public CompletedInterviewsViewModel(
            IPlainStorage<InterviewView> interviewViewRepository,
            IInterviewViewModelFactory viewModelFactory,
            IPrincipal principal) : base(viewModelFactory)
        {
            this.interviewViewRepository = interviewViewRepository;
            this.principal = principal;
        }

        protected override IReadOnlyCollection<InterviewView> GetDbItems()
        {
            var interviewerId = this.principal.CurrentUserIdentity.UserId;

            return this.interviewViewRepository.Where(interview =>
                interview.ResponsibleId == interviewerId &&
                interview.Status == SharedKernels.DataCollection.ValueObjects.Interview.InterviewStatus.Completed);
        }

        protected override void OnItemCreated(InterviewDashboardItemViewModel interviewDashboardItem)
            => interviewDashboardItem.OnItemRemoved += this.InterviewDashboardItem_OnItemRemoved;

        private void InterviewDashboardItem_OnItemRemoved(object sender, System.EventArgs e)
        {
            var dashboardItem = (InterviewDashboardItemViewModel) sender;

            this.ItemsCount--;
            this.UpdateTitle();

            this.UiItems.Remove(dashboardItem);

            this.OnInterviewRemoved(sender,
                new InterviewRemovedArgs(dashboardItem.AssignmentId, dashboardItem.InterviewId));
        }
    }
}