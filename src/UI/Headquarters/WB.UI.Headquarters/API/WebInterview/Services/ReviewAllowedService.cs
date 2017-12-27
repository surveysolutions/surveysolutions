﻿using System;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.WebInterview.Services
{
    class ReviewAllowedService : IReviewAllowedService
    {
        private readonly ITransactionManagerProvider transactionManagerProvider;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage;
        private readonly IAuthorizedUser authorizedUser;

        public ReviewAllowedService(ITransactionManagerProvider transactionManagerProvider,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage,
            IAuthorizedUser authorizedUser)
        {
            this.transactionManagerProvider = transactionManagerProvider;
            this.interviewSummaryStorage = interviewSummaryStorage;
            this.authorizedUser = authorizedUser;
        }

        public void CheckIfAllowed(Guid interviewId)
        {
            var interview = transactionManagerProvider.GetTransactionManager()
                .ExecuteInQueryTransaction(
                    () => interviewSummaryStorage.GetById(interviewId));

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