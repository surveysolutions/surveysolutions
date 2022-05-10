using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.InterviewDataStorage;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Scheduler.Services;

namespace WB.Services.Export.Host.Controllers
{
    [ApiController]
    public class QuestionnaireController : ControllerBase
    {
        private readonly IDatabaseSchemaService databaseSchemaService;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IJobsArchiver archiver;
        private readonly ITenantContext tenantContext;

        public QuestionnaireController(
            IDatabaseSchemaService databaseSchemaService,
            IQuestionnaireStorage questionnaireStorage,
            IJobsArchiver archiver,
            ITenantContext tenantContext)
        {
            this.databaseSchemaService = databaseSchemaService;
            this.questionnaireStorage = questionnaireStorage;
            this.archiver = archiver;
            this.tenantContext = tenantContext;
        }


        [HttpDelete]
        [Route("api/v1/deleteQuestionnaire")]
        public async Task<ActionResult> DeleteQuestionnaire(string questionnaire)
        {
            var questionnaireId = new QuestionnaireId(questionnaire);
            var questionnaireDocument = await questionnaireStorage.GetQuestionnaireAsync(questionnaireId);
            if (questionnaireDocument == null)
                throw new InvalidOperationException($"questionnaire must be not null. {questionnaireId}");

            var result = databaseSchemaService.TryDropQuestionnaireDbStructure(questionnaireDocument);

            var tenantName = tenantContext.Tenant.Name;
            var jobsCount = await this.archiver.ArchiveJobs(tenantName, questionnaire);
            
            return Ok(new
            {
                result = result,
                archivedJobs = jobsCount
            });
        }
    }
}