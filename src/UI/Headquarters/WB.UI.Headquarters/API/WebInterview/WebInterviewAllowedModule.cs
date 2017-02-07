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

namespace WB.UI.Headquarters.API.WebInterview
{
    public class WebInterviewAllowedModule : HubPipelineModule
    {
        private IReadSideKeyValueStorage<InterviewReferences> interviewReferences
            => ServiceLocator.Current.GetInstance<IReadSideKeyValueStorage<InterviewReferences>>();

        private IWebInterviewConfigProvider configProvider =>
            ServiceLocator.Current.GetInstance<IWebInterviewConfigProvider>();

        private IStatefullWebInterviewFactory statefullWebInterviewFactory =>
            ServiceLocator.Current.GetInstance<IStatefullWebInterviewFactory>();

        private IQueryableReadSideRepositoryReader<InterviewSummary> InterviewSummaryStorage => 
            ServiceLocator.Current.GetInstance<IQueryableReadSideRepositoryReader<InterviewSummary>>();

        private IPlainTransactionManager transactionManager
          => ServiceLocator.Current.GetInstance<IPlainTransactionManagerProvider>().GetPlainTransactionManager();

        private ITransactionManager readTransactionManager
            => ServiceLocator.Current.GetInstance<ITransactionManagerProvider>().GetTransactionManager();

        protected override bool OnBeforeConnect(IHub hub)
        {
            QuestionnaireIdentity questionnaireIdentity = null;
            bool interviewIsCompleted = this.readTransactionManager.ExecuteInQueryTransaction(() =>
            {
                var interviewId = this.statefullWebInterviewFactory.GetInterviewIdByHumanId(hub.Context.QueryString.Get(@"interviewId"));

                var questionnaire = this.interviewReferences.GetById(interviewId);
                questionnaireIdentity = new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.QuestionnaireVersion);

                var interviewSummary = this.InterviewSummaryStorage.GetById(interviewId);

                return interviewSummary.Status != InterviewStatus.InterviewerAssigned;
            });

            if (interviewIsCompleted || questionnaireIdentity == null) throw new NotAuthorizedException();

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