using System;
using System.Linq.Expressions;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class CompletedInterviewsViewModel : BaseInterviewsViewModel
    {
        public override GroupStatus InterviewStatus => GroupStatus.Completed;
        public override string TabTitle => InterviewerUIResources.Dashboard_CompletedLinkText;
        public override string TabDescription => InterviewerUIResources.Dashboard_CompletedTabText;
        
        private readonly IPrincipal principal;

        public event EventHandler<InterviewRemovedArgs> OnInterviewRemoved;

        public CompletedInterviewsViewModel(
            IPlainStorage<InterviewView> interviewViewRepository,
            IInterviewViewModelFactory viewModelFactory,
            IPlainStorage<PrefilledQuestionView> identifyingQuestionsRepo,
            IPrincipal principal) : base(viewModelFactory, interviewViewRepository, identifyingQuestionsRepo)
        {
            this.principal = principal;
        }

        protected override Expression<Func<InterviewView, bool>> GetDbQuery()
        {
            var interviewerId = this.principal.CurrentUserIdentity.UserId;

            return interview => interview.ResponsibleId == interviewerId &&
                                interview.Status == SharedKernels.DataCollection.ValueObjects.Interview.InterviewStatus.Completed;
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
