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
    public class RejectedInterviewsViewModel : ListViewModel<IDashboardItem>
    {
        public override GroupStatus InterviewStatus => GroupStatus.StartedInvalid;

        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IInterviewViewModelFactory viewModelFactory;
        private readonly IPrincipal principal;

        private IReadOnlyCollection<InterviewView> dbItems;

        public RejectedInterviewsViewModel(
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
            this.Title = string.Format(InterviewerUIResources.Dashboard_RejectedLinkText, this.ItemsCount);

            var uiItems = await Task.Run(() => this.GetUiItems());
            this.UiItems.ReplaceWith(uiItems);
        }

        private IReadOnlyCollection<InterviewView> GetDbItems()
        {
            var interviewerId = this.principal.CurrentUserIdentity.UserId;

            return this.interviewViewRepository.Where(interview =>
                interview.ResponsibleId == interviewerId &&
                interview.Status == SharedKernels.DataCollection.ValueObjects.Interview.InterviewStatus.RejectedBySupervisor);
        }

        private IEnumerable<IDashboardItem> GetUiItems()
        {
            var subTitle = this.viewModelFactory.GetNew<DashboardSubTitleViewModel>();
            subTitle.Title = InterviewerUIResources.Dashboard_RejectedTabText;

            yield return subTitle;

            foreach (var interviewView in this.dbItems)
            {
                var interviewDashboardItem = this.viewModelFactory.GetNew<InterviewDashboardItemViewModel>();
                interviewDashboardItem.Init(interviewView);

                yield return interviewDashboardItem;
            }
        }
    }
}