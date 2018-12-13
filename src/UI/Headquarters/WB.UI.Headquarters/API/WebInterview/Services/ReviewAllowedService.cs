using System;
using System.Runtime.Caching;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Enumerator.Native.WebInterview;

namespace WB.UI.Headquarters.API.WebInterview.Services
{
    class ReviewAllowedService : IReviewAllowedService
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage;
        private readonly IAuthorizedUser authorizedUser;

        private static readonly MemoryCache reviewAllowedCache = new MemoryCache("ReviewAllowedServiceInterviewsCache");

        public ReviewAllowedService(IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage,
            IAuthorizedUser authorizedUser)
        {
            this.interviewSummaryStorage = interviewSummaryStorage;
            this.authorizedUser = authorizedUser;
        }

        public void CheckIfAllowed(Guid interviewId)
        {
            var interview = (InterviewSummary)reviewAllowedCache.Get(interviewId.FormatGuid());
            if (interview == null)
            {
                interview = interviewSummaryStorage.GetById(interviewId);
                if (interview != null)
                {
                    reviewAllowedCache.Set(interviewId.FormatGuid(), interview, DateTimeOffset.UtcNow.AddSeconds(5));
                }
            }

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
