using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.Headquarters.API.WebInterview.Pipeline
{
    public class WebInterviewAllowedModule : HubPipelineModule
    {
        private IWebInterviewConfigProvider configProvider =>
            ServiceLocator.Current.GetInstance<IWebInterviewConfigProvider>();

        private IStatefullWebInterviewFactory statefullWebInterviewFactory =>
            ServiceLocator.Current.GetInstance<IStatefullWebInterviewFactory>();
        
        private IPlainTransactionManager transactionManager
          => ServiceLocator.Current.GetInstance<IPlainTransactionManagerProvider>().GetPlainTransactionManager();

        private ITransactionManager readTransactionManager
            => ServiceLocator.Current.GetInstance<ITransactionManagerProvider>().GetTransactionManager();

        protected override bool OnBeforeConnect(IHub hub)
        {
            QuestionnaireIdentity questionnaireIdentity = null;
            bool interviewAcceptAnswers = this.readTransactionManager.ExecuteInQueryTransaction(() =>
            {
                var interview = this.statefullWebInterviewFactory.Get(hub.Context.QueryString.Get(@"interviewId"));
                questionnaireIdentity = interview?.QuestionnaireIdentity;
                return interview?.AcceptsInterviewerAnswers() ?? false;
            });

            if (!interviewAcceptAnswers || questionnaireIdentity == null) throw new NotAuthorizedException();

            WebInterviewConfig webInterviewConfig = null;
            this.transactionManager.ExecuteInPlainTransaction(() =>
            {
                webInterviewConfig = this.configProvider.Get(questionnaireIdentity);
            });

            if (!webInterviewConfig.Started) throw new NotAuthorizedException();

            return base.OnBeforeConnect(hub);
        }
    }
}