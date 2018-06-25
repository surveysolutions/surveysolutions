using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel
{
    public class DashboardRejectedInterviewsViewModel : BaseInterviewsViewModel
    {
        private readonly IPrincipal principal;

        public override Task Initialize()
        {
            this.Load(null);
            return Task.CompletedTask;
        }

        public DashboardRejectedInterviewsViewModel(IInterviewViewModelFactory viewModelFactory,
            IPlainStorage<InterviewView> interviewViewRepository,
            IPlainStorage<PrefilledQuestionView> identifyingQuestionsRepo,
            IPrincipal principal) : 
                base(viewModelFactory, interviewViewRepository, identifyingQuestionsRepo)
        {
            this.principal = principal;
        }

        public override GroupStatus InterviewStatus => GroupStatus.Completed;

        public override string TabTitle => SupervisorDashboard.Rejected;

        public override string TabDescription { get; }

        protected override Expression<Func<InterviewView, bool>> GetDbQuery()
        {
            var currentUserId = this.principal.CurrentUserIdentity.UserId;

            return interview => interview.ResponsibleId == currentUserId &&
                                interview.Status == SharedKernels.DataCollection.ValueObjects.Interview.InterviewStatus.RejectedByHeadquarters;
        }
    }
}
