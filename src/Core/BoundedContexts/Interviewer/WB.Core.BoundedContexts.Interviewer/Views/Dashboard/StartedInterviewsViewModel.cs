using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class StartedInterviewsViewModel : ListViewModel<InterviewDashboardItemViewModel>
    {
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IInterviewViewModelFactory viewModelFactory;
        private readonly IPrincipal principal;

        public event EventHandler OnInterviewRemoved;

        public StartedInterviewsViewModel(
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
            this.Items = this.GetStartedInterviews().ToList();
            this.Title = string.Format(InterviewerUIResources.Dashboard_StartedLinkText, this.Items.Count);
        }

        private IEnumerable<InterviewDashboardItemViewModel> GetStartedInterviews()
        {
            var interviewerId = this.principal.CurrentUserIdentity.UserId;

            var interviewViews = this.interviewViewRepository.Where(interview =>
                interview.ResponsibleId == interviewerId &&
                (interview.Status == InterviewStatus.InterviewerAssigned && interview.StartedDateTime != null || 
                interview.Status == InterviewStatus.Restarted));

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
            this.Load();
            this.OnInterviewRemoved(sender, e);
        }
    }
}