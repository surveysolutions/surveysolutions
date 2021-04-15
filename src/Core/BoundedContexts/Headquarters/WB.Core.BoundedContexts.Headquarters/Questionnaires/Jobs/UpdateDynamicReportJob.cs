using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.Services.DynamicReporting;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Questionnaires.Jobs
{
    [DisallowConcurrentExecution]
    [RetryFailedJob]
    [DisplayName("Update dynamic report job"), Category("Update")]
    public class UpdateDynamicReportJob : IJob<UpdateDynamicReportRequest>
    {
        private readonly IExposedVariablesService exposedVariablesService;
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemReader;
        private readonly ILogger<DeleteQuestionnaireJob> logger;

        public UpdateDynamicReportJob(
            IExposedVariablesService exposedVariablesService, 
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemReader,
            ILogger<DeleteQuestionnaireJob> logger)
        {
            this.exposedVariablesService = exposedVariablesService;
            this.questionnaireBrowseItemReader = questionnaireBrowseItemReader;
            this.logger = logger;
        }

        public Task Execute(UpdateDynamicReportRequest data, IJobExecutionContext context)
        {
            
            var questionnaire = this.questionnaireBrowseItemReader.GetById(data.Identity.ToString());

            if (questionnaire == null || questionnaire.IsDeleted)
            {
                logger.LogWarning("Questionnaire is deleted: {identity}", data.Identity);
                return Task.CompletedTask;
            }

            return exposedVariablesService.UpdateDynamicReportDataAsync(
                data.Identity);
        }
    }
    
}
