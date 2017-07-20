using System;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
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
        private readonly HqUserManager hqUserManager;

        public WebInterviewAllowService(
            ITransactionManagerProvider transactionManagerProvider,
            IPlainTransactionManagerProvider plainTransactionManagerProvider,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage,
            IWebInterviewConfigProvider webInterviewConfigProvider,
            HqUserManager hqUserManager)
        {
            this.transactionManagerProvider = transactionManagerProvider;
            this.interviewSummaryStorage = interviewSummaryStorage;
            this.webInterviewConfigProvider = webInterviewConfigProvider;
            this.hqUserManager = hqUserManager;
            this.plainTransactionManagerProvider = plainTransactionManagerProvider;
        }

        public void CheckWebInterviewAccessPermissions(string interviewId, Guid? userId)
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

            QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(interview.QuestionnaireId, interview.QuestionnaireVersion);

            WebInterviewConfig webInterviewConfig = plainTransactionManagerProvider
                .GetPlainTransactionManager()
                    .ExecuteInPlainTransaction(
                        () => webInterviewConfigProvider.Get(questionnaireIdentity));

            if (!webInterviewConfig.Started)
            {
                if (userId.HasValue)
                {
                    var user = hqUserManager.FindById(userId.Value);

                    if (user.IsInRole(UserRoles.Interviewer) && user.Id == interview.ResponsibleId)
                    {
                        return;
                    }
                }
                
                throw new WebInterviewAccessException(InterviewAccessExceptionReason.InterviewExpired,
                Headquarters.Resources.WebInterview.Error_InterviewExpired);
            }
        }
    }
}
