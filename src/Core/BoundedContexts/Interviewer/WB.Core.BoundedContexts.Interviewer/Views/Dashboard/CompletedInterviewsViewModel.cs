using System;
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
    public class CompletedInterviewsViewModel : ListViewModel<IDashboardItem>
    {
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

            this.Title = string.Format(InterviewerUIResources.Dashboard_CompletedLinkText, completedInterviews.Count);

            var subTitle = this.viewModelFactory.GetNew<DashboardSubTitleViewModel>();
            subTitle.Title = InterviewerUIResources.Dashboard_CompletedTabText;
            var uiItems = subTitle.ToEnumerable().Concat(completedInterviews).ToList();

            this.Items = completedInterviews;
            this.UiItems = new MvxObservableCollection<IDashboardItem>(uiItems);
        }

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

            this.Items.Remove(dashboardItem);
            this.UiItems.Remove(dashboardItem);

            this.OnInterviewRemoved(sender, e);
        }
    }
}