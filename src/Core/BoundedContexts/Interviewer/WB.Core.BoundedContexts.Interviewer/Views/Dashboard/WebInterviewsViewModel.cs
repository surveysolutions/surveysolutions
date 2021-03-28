using System;
using System.Linq.Expressions;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class WebInterviewsViewModel : BaseInterviewsViewModel
    {
        public override GroupStatus InterviewStatus => GroupStatus.WebInterview;
        public override string TabTitle => EnumeratorUIResources.Dashboard_WebInterviewsLinkText;
        public override string TabDescription => EnumeratorUIResources.Dashboard_WebInterviewTabText;

        public event EventHandler<InterviewRemovedArgs>? OnInterviewRemoved;

        public WebInterviewsViewModel(
            IPlainStorage<InterviewView> interviewViewRepository,
            IInterviewViewModelFactory viewModelFactory,
            IPlainStorage<PrefilledQuestionView> identifyingQuestionsRepo,
            IPrincipal principal) : base(viewModelFactory, interviewViewRepository, identifyingQuestionsRepo, principal)
        {
        }

        protected override Expression<Func<InterviewView, bool>> GetDbQuery()
        {
            var interviewerId = Principal.CurrentUserIdentity.UserId;

            return interview => interview.ResponsibleId == interviewerId &&
                                interview.Mode == InterviewMode.CAWI;
        }

        protected override void OnItemCreated(InterviewDashboardItemViewModel interviewDashboardItem)
        {
            base.OnItemCreated(interviewDashboardItem);
            interviewDashboardItem.OnItemRemoved += this.InterviewDashboardItem_OnItemRemoved;
        }

        private void InterviewDashboardItem_OnItemRemoved(object sender, System.EventArgs e)
        {
            var dashboardItem = (InterviewDashboardItemViewModel)sender;

            this.ItemsCount--;
            this.UpdateTitle();

            this.UiItems.Remove(dashboardItem);

            this.OnInterviewRemoved?.Invoke(sender,
                new InterviewRemovedArgs(dashboardItem.AssignmentId, dashboardItem.InterviewId));
        }
    }
}
