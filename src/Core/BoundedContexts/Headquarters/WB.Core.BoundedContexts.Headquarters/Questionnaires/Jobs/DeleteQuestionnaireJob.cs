using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Questionnaires.Jobs
{
    [DisallowConcurrentExecution]
    [RetryFailedJob]
    [DisplayName("Delete questionnaire job"), Category("Cleanup")]
    public class DeleteQuestionnaireJob : IJob<DeleteQuestionnaireRequest>
    {
        private readonly IDeleteQuestionnaireService deleteQuestionnaireService;
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemReader;
        private readonly ILogger<DeleteQuestionnaireJob> logger;

        public DeleteQuestionnaireJob(
            IDeleteQuestionnaireService deleteQuestionnaireService, 
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemReader,
            ILogger<DeleteQuestionnaireJob> logger)
        {
            this.deleteQuestionnaireService = deleteQuestionnaireService;
            this.questionnaireBrowseItemReader = questionnaireBrowseItemReader;
            this.logger = logger;
        }

        public Task Execute(DeleteQuestionnaireRequest data, IJobExecutionContext context)
        {
            var questionnaire = this.questionnaireBrowseItemReader.GetById(
                new QuestionnaireIdentity(data.QuestionnaireId, data.Version).ToString());

            if (questionnaire == null || questionnaire.IsDeleted)
            {
                logger.LogWarning("Requested questionnaire is already deleted: {id}${version}", data.QuestionnaireId, data.Version);
                return Task.CompletedTask;
            }

            if (!questionnaire.DisabledBy.HasValue)
            {
                logger.LogError("Should specify userId for questionnaire to delete");
                return Task.CompletedTask;
            }

            if (questionnaire.DisabledBy != data.UserId)
            {
                logger.LogError("Requested user to delete in questionnaire and in scheduled job is different");
            }

            return deleteQuestionnaireService.DeleteInterviewsAndQuestionnaireAfterAsync(
                data.QuestionnaireId, 
                data.Version, 
                data.UserId);
        }
    }
    
}
