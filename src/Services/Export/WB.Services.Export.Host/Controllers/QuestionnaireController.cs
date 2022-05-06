using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WB.Services.Export.InterviewDataStorage;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;

namespace WB.Services.Export.Host.Controllers
{
    [ApiController]
    public class QuestionnaireController : ControllerBase
    {
        private readonly IDatabaseSchemaService databaseSchemaService;
        private readonly IQuestionnaireStorage questionnaireStorage;

        public QuestionnaireController(
            IDatabaseSchemaService databaseSchemaService,
            IQuestionnaireStorage questionnaireStorage)
        {
            this.databaseSchemaService = databaseSchemaService;
            this.questionnaireStorage = questionnaireStorage;
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

            return Ok(new
            {
                result = result
            });
        }
    }
}