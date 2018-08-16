using System.Linq;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Infrastructure.Native.Threading;

namespace WB.Core.BoundedContexts.Headquarters.Questionnaires.Jobs
{
    [DisallowConcurrentExecution]
    public class DeleteQuestionnaireJob : IJob
    {
        private IPlainStorageAccessor<QuestionnaireBrowseItem> QuestionnaireBrowseItemReader => 
            ServiceLocator.Current.GetInstance<IPlainStorageAccessor<QuestionnaireBrowseItem>>();

        private IPlainTransactionManager plainTransactionManager =>
            ServiceLocator.Current.GetInstance<IPlainTransactionManager>();


        private IDeleteQuestionnaireService DeletionService => ServiceLocator.Current.GetInstance<IDeleteQuestionnaireService>();

        public void Execute(IJobExecutionContext context)
        {
            var disabledNotDeletedQuestionnaire =
                this.plainTransactionManager.ExecuteInQueryTransaction(() => 
                QuestionnaireBrowseItemReader.Query(_ => _.FirstOrDefault(q => q.Disabled && !q.IsDeleted)));

            if(disabledNotDeletedQuestionnaire == null) return;

            ThreadMarkerManager.MarkCurrentThreadAsIsolated();
            try
            {
                this.DeletionService.DeleteInterviewsAndQuestionnaireAfter(disabledNotDeletedQuestionnaire.QuestionnaireId, disabledNotDeletedQuestionnaire.Version, disabledNotDeletedQuestionnaire.DisabledBy);
            }
            finally
            {
                ThreadMarkerManager.ReleaseCurrentThreadFromIsolation();
            }
        }
    }
}
