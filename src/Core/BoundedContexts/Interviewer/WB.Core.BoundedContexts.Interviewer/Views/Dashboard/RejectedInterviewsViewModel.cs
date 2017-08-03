using System.Collections.Generic;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class RejectedInterviewsViewModel : BaseInterviewsViewModel
    {
        public override GroupStatus InterviewStatus => GroupStatus.StartedInvalid;
        protected override string TabTitle => InterviewerUIResources.Dashboard_RejectedLinkText;
        protected override string TabDescription => InterviewerUIResources.Dashboard_RejectedTabText;

        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IPrincipal principal;

        public RejectedInterviewsViewModel(
            IPlainStorage<InterviewView> interviewViewRepository,
            IInterviewViewModelFactory viewModelFactory,
            IPrincipal principal) : base(viewModelFactory)
        {
            this.interviewViewRepository = interviewViewRepository;
            this.principal = principal;
        }

        protected override IReadOnlyCollection<InterviewView> GetDbItems()
        {
            var interviewerId = this.principal.CurrentUserIdentity.UserId;

            return this.interviewViewRepository.Where(interview =>
                interview.ResponsibleId == interviewerId &&
                interview.Status == SharedKernels.DataCollection.ValueObjects.Interview.InterviewStatus.RejectedBySupervisor);
        }
    }
}