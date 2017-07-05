using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class CompletedInterviewsViewModel : ListViewModel<IDashboardItem>
    {
        public override GroupStatus InterviewStatus => GroupStatus.Completed;

        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IInterviewViewModelFactory viewModelFactory;
        private readonly IPrincipal principal;

        public event EventHandler<InterviewRemovedArgs> OnInterviewRemoved;
        private IReadOnlyCollection<InterviewView> dbItems;

        public CompletedInterviewsViewModel(
            IPlainStorage<InterviewView> interviewViewRepository,
            IInterviewViewModelFactory viewModelFactory,
            IPrincipal principal)
        {
            this.interviewViewRepository = interviewViewRepository;
            this.viewModelFactory = viewModelFactory;
            this.principal = principal;
        }

        public async void Load()
        {
            this.dbItems = this.GetDbItems();

            this.ItemsCount = this.dbItems.Count;
            this.UpdateTitle();

            var uiItems = await Task.Run(() => this.GetUiItems());
            this.UiItems.ReplaceWith(uiItems);
        }

        private void UpdateTitle() => 
            this.Title = string.Format(InterviewerUIResources.Dashboard_CompletedLinkText, this.ItemsCount);

        private IReadOnlyCollection<InterviewView> GetDbItems()
        {
            var interviewerId = this.principal.CurrentUserIdentity.UserId;

            return this.interviewViewRepository.Where(interview =>
                interview.ResponsibleId == interviewerId &&
                interview.Status == SharedKernels.DataCollection.ValueObjects.Interview.InterviewStatus.Completed);
        }

        private IEnumerable<IDashboardItem> GetUiItems()
        {
            var subTitle = this.viewModelFactory.GetNew<DashboardSubTitleViewModel>();
            subTitle.Title = InterviewerUIResources.Dashboard_CompletedTabText;

            yield return subTitle;

            foreach (var interviewView in this.dbItems)
            {
                var interviewDashboardItem = this.viewModelFactory.GetNew<InterviewDashboardItemViewModel>();
                interviewDashboardItem.Init(interviewView);
                interviewDashboardItem.OnItemRemoved += this.InterviewDashboardItem_OnItemRemoved;
                yield return interviewDashboardItem;
            }
        }

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