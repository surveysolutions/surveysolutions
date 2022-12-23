using System;
using System.Linq.Expressions;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class StartedInterviewsViewModel : BaseInterviewsViewModel
    {
        public override GroupStatus InterviewStatus => GroupStatus.Started;
        public override string TabTitle => EnumeratorUIResources.Dashboard_StartedLinkText;
        public override string TabDescription => EnumeratorUIResources.Dashboard_StartedTabText;
        
        public event EventHandler<InterviewRemovedArgs>? OnInterviewRemoved;

        public StartedInterviewsViewModel(IPlainStorage<InterviewView> interviewViewRepository, 
            IInterviewViewModelFactory viewModelFactory,
            IPlainStorage<PrefilledQuestionView> identifyingQuestionsRepo,
            IPrincipal principal) : base(viewModelFactory, interviewViewRepository, identifyingQuestionsRepo, principal)
        {
        }

        protected override Expression<Func<InterviewView, bool>> GetDbQuery()
        {
            var interviewerId = this.Principal.CurrentUserIdentity.UserId;

            return interview => interview.ResponsibleId == interviewerId &&
                                (interview.Mode == InterviewMode.CAPI || interview.Mode == null) && 
                (interview.Status == SharedKernels.DataCollection.ValueObjects.Interview.InterviewStatus.InterviewerAssigned ||
                 interview.Status == SharedKernels.DataCollection.ValueObjects.Interview.InterviewStatus.Restarted);
        }

        protected override void OnItemCreated(InterviewDashboardItemViewModel interviewDashboardItem)
        {
            base.OnItemCreated(interviewDashboardItem);
            interviewDashboardItem.OnItemRemoved += this.InterviewDashboardItem_OnItemRemoved;
        }

        private async void InterviewDashboardItem_OnItemRemoved(object sender, EventArgs e)
        {
            var dashboardItem = (InterviewDashboardItemViewModel)sender;

            this.ItemsCount--;
            this.UpdateTitle();

            await this.InvokeOnMainThreadAsync(() =>
            {
                this.UiItems.Remove(dashboardItem);

            }, false).ConfigureAwait(false);

            if (this.OnInterviewRemoved != null)
            {
                this.OnInterviewRemoved(sender,
                    new InterviewRemovedArgs(dashboardItem.AssignmentId, dashboardItem.InterviewId));
            }
        }
    }
}
