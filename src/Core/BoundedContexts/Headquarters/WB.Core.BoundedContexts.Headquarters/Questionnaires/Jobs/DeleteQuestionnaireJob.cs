using System;
using System.Linq;
using System.Threading.Tasks;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Questionnaires.Jobs
{
 [DisallowConcurrentExecution]
    public class DeleteQuestionnaireJob : IJob
    {
        private readonly IDeleteQuestionnaireService deleteQuestionnaireService;
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemReader;

        public DeleteQuestionnaireJob(IDeleteQuestionnaireService deleteQuestionnaireService, 
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemReader)
        {
            this.deleteQuestionnaireService = deleteQuestionnaireService;
            this.questionnaireBrowseItemReader = questionnaireBrowseItemReader;
        }

        public Task Execute(IJobExecutionContext context)
        {
            var disabledNotDeletedQuestionnaire =
                    questionnaireBrowseItemReader.Query(_ => _.FirstOrDefault(q => q.Disabled && !q.IsDeleted));

            if(disabledNotDeletedQuestionnaire == null) return Task.CompletedTask;

            if (!disabledNotDeletedQuestionnaire.DisabledBy.HasValue)
                throw new ArgumentException("Should specify userId for delete questionnaire");
                
            return deleteQuestionnaireService.DeleteInterviewsAndQuestionnaireAfterAsync(disabledNotDeletedQuestionnaire.QuestionnaireId, disabledNotDeletedQuestionnaire.Version, disabledNotDeletedQuestionnaire.DisabledBy.Value);
        }
    }
}
