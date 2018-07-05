using System;
using System.Linq.Expressions;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class RejectedInterviewsViewModel : BaseInterviewsViewModel
    {
        public override GroupStatus InterviewStatus => GroupStatus.StartedInvalid;
        public override string TabTitle => InterviewerUIResources.Dashboard_RejectedLinkText;
        public override string TabDescription => InterviewerUIResources.Dashboard_RejectedTabText;
        protected override Expression<Func<InterviewView, bool>> GetDbQuery()
        {
            var interviewerId = this.principal.CurrentUserIdentity.UserId;

            return interview => interview.ResponsibleId == interviewerId &&
                                interview.Status == SharedKernels.DataCollection.ValueObjects.Interview.InterviewStatus.RejectedBySupervisor;
        }
        
        private readonly IPrincipal principal;

        public RejectedInterviewsViewModel(IPlainStorage<InterviewView> interviewViewRepository, 
            IInterviewViewModelFactory viewModelFactory,
            IPlainStorage<PrefilledQuestionView> identifyingQuestionsRepo,
            IPrincipal principal) : base(viewModelFactory, interviewViewRepository, identifyingQuestionsRepo)
        {
            this.principal = principal;
        }
    }
}
