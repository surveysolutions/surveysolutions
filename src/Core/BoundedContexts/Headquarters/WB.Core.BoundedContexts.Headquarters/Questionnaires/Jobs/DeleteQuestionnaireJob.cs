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
        private readonly IDeleteQuestionnaireService deleteQuestionnaireService;
        private readonly IPlainTransactionManager transactionManager;
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemReader;

        public DeleteQuestionnaireJob(IDeleteQuestionnaireService deleteQuestionnaireService, 
            IPlainTransactionManager transactionManager, 
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemReader)
        {
            this.deleteQuestionnaireService = deleteQuestionnaireService;
            this.transactionManager = transactionManager;
            this.questionnaireBrowseItemReader = questionnaireBrowseItemReader;
        }

        public void Execute(IJobExecutionContext context)
        {
            var disabledNotDeletedQuestionnaire =
                transactionManager.ExecuteInQueryTransaction(() =>
                    questionnaireBrowseItemReader.Query(_ => _.FirstOrDefault(q => q.Disabled && !q.IsDeleted)));

            if(disabledNotDeletedQuestionnaire == null) return;

            deleteQuestionnaireService.DeleteInterviewsAndQuestionnaireAfter(disabledNotDeletedQuestionnaire.QuestionnaireId, disabledNotDeletedQuestionnaire.Version, disabledNotDeletedQuestionnaire.DisabledBy);
        }
    }
}
