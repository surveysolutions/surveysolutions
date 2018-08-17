using System;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Enumerator.Native.WebInterview;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.WebInterview.Services
{
    class ReviewAllowedService : IReviewAllowedService
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage;
        private readonly IAuthorizedUser authorizedUser;

        public ReviewAllowedService(IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage,
            IAuthorizedUser authorizedUser)
        {
            this.interviewSummaryStorage = interviewSummaryStorage;
            this.authorizedUser = authorizedUser;
        }

        public void CheckIfAllowed(Guid interviewId)
        {
            var interview = interviewSummaryStorage.GetById(interviewId);

            if (interview == null)
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewNotFound, Enumerator.Native.Resources.WebInterview.Error_NotFound);

            if (!CurrentUserCanAccessInterview(interview))
            {
                throw new InterviewAccessException(InterviewAccessExceptionReason.UserNotAuthorised, Enumerator.Native.Resources.WebInterview.Error_UserNotAuthorised);
            }
        }

        private bool CurrentUserCanAccessInterview(InterviewSummary interviewSummary)
        {
            return this.authorizedUser.IsAdministrator || this.authorizedUser.IsHeadquarter ||
                   (this.authorizedUser.IsSupervisor && this.authorizedUser.Id == interviewSummary.TeamLeadId);
        }
    }
}
