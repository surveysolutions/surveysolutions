using System;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview;

namespace WB.UI.Headquarters.Services.Impl
{
    class ReviewAllowedService : IReviewAllowedService
    {
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IAuthorizedUser authorizedUser;

        public ReviewAllowedService(IStatefulInterviewRepository statefulInterviewRepository,
            IAuthorizedUser authorizedUser)
        {
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.authorizedUser = authorizedUser;
        }

        public void CheckIfAllowed(Guid interviewId)
        {
            var interview = statefulInterviewRepository.Get(interviewId.FormatGuid());
            if (interview == null)
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewNotFound, Enumerator.Native.Resources.WebInterview.Error_NotFound);

            if (!CurrentUserCanAccessInterview(interview))
            {
                throw new InterviewAccessException(InterviewAccessExceptionReason.UserNotAuthorised, Enumerator.Native.Resources.WebInterview.Error_UserNotAuthorised);
            }
        }

        private bool CurrentUserCanAccessInterview(IStatefulInterview interview)
        {
            return this.authorizedUser.IsAdministrator || this.authorizedUser.IsHeadquarter ||
                   (this.authorizedUser.IsSupervisor && this.authorizedUser.Id == interview.SupervisorId);
        }
    }
}
