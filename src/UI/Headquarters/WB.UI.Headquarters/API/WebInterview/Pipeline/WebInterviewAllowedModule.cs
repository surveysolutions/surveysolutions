using System;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.Services.WebInterview;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.WebInterview.Pipeline
{
    public class WebInterviewAllowedModule : HubPipelineModule
    {
        private IWebInterviewConfigProvider configProvider =>
            ServiceLocator.Current.GetInstance<IWebInterviewConfigProvider>();

        private IStatefulInterviewRepository statefullInterviewRepository =>
            ServiceLocator.Current.GetInstance<IStatefulInterviewRepository>();

        private IWebInterviewNotificationService webInterviewNotificationService =>
            ServiceLocator.Current.GetInstance<IWebInterviewNotificationService>();

        private IPlainTransactionManager transactionManager
          => ServiceLocator.Current.GetInstance<IPlainTransactionManagerProvider>().GetPlainTransactionManager();

        private ITransactionManager readTransactionManager
            => ServiceLocator.Current.GetInstance<ITransactionManagerProvider>().GetTransactionManager();

        protected override bool OnBeforeConnect(IHub hub)
        {
            this.CheckWebInterviewAccessPermissions(hub);

            return base.OnBeforeConnect(hub);
        }

        protected override bool OnBeforeIncoming(IHubIncomingInvokerContext context)
        {
            var hub = context.Hub;

            try
            {
                this.CheckWebInterviewAccessPermissions(hub);
            }
            catch (WebInterviewAccessException)
            {
                var interviewId = hub.Context.QueryString.Get(@"interviewId");
                if (!interviewId.IsNullOrWhiteSpace())
                {
                    webInterviewNotificationService.ReloadInterview(Guid.Parse(interviewId));
                }
            }

            return base.OnBeforeIncoming(context);
        }

        private void CheckWebInterviewAccessPermissions(IHub hub)
        {
            var interview = this.readTransactionManager.ExecuteInQueryTransaction(
                () => this.statefullInterviewRepository.Get(hub.Context.QueryString.Get(@"interviewId")));

            if (interview == null)
                throw new WebInterviewAccessException(InterviewAccessExceptionReason.InterviewNotFound, WB.UI.Headquarters.Resources.WebInterview.Error_NotFound);

            if (interview.IsDeleted)
                throw new WebInterviewAccessException(InterviewAccessExceptionReason.InterviewExpired, WB.UI.Headquarters.Resources.WebInterview.Error_InterviewExpired);

            if (interview.Status != InterviewStatus.InterviewerAssigned)
                throw new WebInterviewAccessException(InterviewAccessExceptionReason.NoActionsNeeded, WB.UI.Headquarters.Resources.WebInterview.Error_NoActionsNeeded);

            QuestionnaireIdentity questionnaireIdentity = interview.QuestionnaireIdentity;


            WebInterviewConfig webInterviewConfig = this.transactionManager.ExecuteInPlainTransaction(
                () => this.configProvider.Get(questionnaireIdentity));

            if (!webInterviewConfig.Started)
                throw new WebInterviewAccessException(InterviewAccessExceptionReason.InterviewExpired, WB.UI.Headquarters.Resources.WebInterview.Error_InterviewExpired);
        }
    }
}