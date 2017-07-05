using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class CompletedInterviewsViewModel : ListViewModel<IDashboardItem>
    {
        public override int ItemsCount => this.UiItems.OfType<InterviewDashboardItemViewModel>().Count();

        public override GroupStatus InterviewStatus => GroupStatus.Completed;

        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IInterviewViewModelFactory viewModelFactory;
        private readonly IPrincipal principal;

        public event EventHandler OnInterviewRemoved;

        public CompletedInterviewsViewModel(
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
            var completedInterviews = await Task.Run(() => this.GetCompletedInterviews().ToList());

            var subTitle = this.viewModelFactory.GetNew<DashboardSubTitleViewModel>();
            subTitle.Title = InterviewerUIResources.Dashboard_CompletedTabText;
            var uiItems = subTitle.ToEnumerable().Concat(completedInterviews).ToList();
            
            this.UiItems.ReplaceWith(uiItems);

            this.UpdateTitle();
        }

        private void UpdateTitle() => 
            this.Title = string.Format(InterviewerUIResources.Dashboard_CompletedLinkText, this.ItemsCount);

        private IEnumerable<IDashboardItem> GetCompletedInterviews()
        {
            var interviewerId = this.principal.CurrentUserIdentity.UserId;

            var interviewViews = this.interviewViewRepository.Where(interview =>
                interview.ResponsibleId == interviewerId &&
                interview.Status == SharedKernels.DataCollection.ValueObjects.Interview.InterviewStatus.Completed);

            foreach (var interviewView in interviewViews)
            {
                var interviewDashboardItem = this.viewModelFactory.GetNew<InterviewDashboardItemViewModel>();
                interviewDashboardItem.Init(interviewView);
                interviewDashboardItem.OnItemRemoved += this.InterviewDashboardItem_OnItemRemoved;
                yield return interviewDashboardItem;
            }
        }

        private void InterviewDashboardItem_OnItemRemoved(object sender, System.EventArgs e)
        {
            var dashboardItem = (IDashboardItem) sender;
            
            this.UiItems.Remove(dashboardItem);

            this.UpdateTitle();

            this.OnInterviewRemoved(sender, e);
        }
    }
}