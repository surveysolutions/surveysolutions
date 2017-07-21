using System;
using System.Threading;
using Main.Core.Entities.SubEntities;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.WebInterview.Services
{
    public class WebInterviewAllowService : IWebInterviewAllowService
    {
        private readonly ITransactionManagerProvider transactionManagerProvider;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage;
        private readonly IWebInterviewConfigProvider webInterviewConfigProvider;
        private readonly IPlainTransactionManagerProvider plainTransactionManagerProvider;

        public WebInterviewAllowService(
            ITransactionManagerProvider transactionManagerProvider,
            IPlainTransactionManagerProvider plainTransactionManagerProvider,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage,
            IWebInterviewConfigProvider webInterviewConfigProvider)
        {
            this.transactionManagerProvider = transactionManagerProvider;
            this.interviewSummaryStorage = interviewSummaryStorage;
            this.webInterviewConfigProvider = webInterviewConfigProvider;
            this.plainTransactionManagerProvider = plainTransactionManagerProvider;
        }

        public void CheckWebInterviewAccessPermissions(string interviewId)
        {
            var interview = transactionManagerProvider.GetTransactionManager()
                .ExecuteInQueryTransaction(
                    () => interviewSummaryStorage.GetById(interviewId));

            if (interview == null)
                throw new WebInterviewAccessException(InterviewAccessExceptionReason.InterviewNotFound, Headquarters.Resources.WebInterview.Error_NotFound);

            if (interview.IsDeleted)
                throw new WebInterviewAccessException(InterviewAccessExceptionReason.InterviewExpired, Headquarters.Resources.WebInterview.Error_InterviewExpired);

            if (interview.Status != InterviewStatus.InterviewerAssigned && interview.Status != InterviewStatus.Restarted)
                throw new WebInterviewAccessException(InterviewAccessExceptionReason.NoActionsNeeded, Headquarters.Resources.WebInterview.Error_NoActionsNeeded);

            var currentPrincipalIdentity = Thread.CurrentPrincipal;
            var userId = currentPrincipalIdentity.Identity.GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                if (currentPrincipalIdentity.IsInRole(UserRoles.Interviewer.ToString()) &&
                    interview.ResponsibleId.ToString() == userId)
                {
                    return;
                }
            }

            QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(interview.QuestionnaireId, interview.QuestionnaireVersion);

            WebInterviewConfig webInterviewConfig = plainTransactionManagerProvider
                .GetPlainTransactionManager()
                    .ExecuteInPlainTransaction(
                        () => webInterviewConfigProvider.Get(questionnaireIdentity));

            if (!webInterviewConfig.Started)
            {
                throw new WebInterviewAccessException(InterviewAccessExceptionReason.InterviewExpired,
                Headquarters.Resources.WebInterview.Error_InterviewExpired);
            }
        }
    }
}
