using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Designer.QuestionnaireCompilationForOldVersions;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.UI.Designer.Controllers.Api.Designer;

namespace WB.UI.Designer.Api
{
    [Authorize]
    [QuestionnairePermissions]
    public class ExpressionGenerationController : Controller
    {
        private readonly IExpressionProcessorGenerator expressionProcessorGenerator;
        private readonly ILogger<ExpressionGenerationController> logger;
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly IDesignerEngineVersionService engineVersionService;
        private IQuestionnaireCompilationVersionService questionnaireCompilationVersionService;

        public ExpressionGenerationController(ILogger<ExpressionGenerationController> logger, 
            IExpressionProcessorGenerator expressionProcessorGenerator, 
            IQuestionnaireViewFactory questionnaireViewFactory, 
            IDesignerEngineVersionService engineVersionService, 
            IQuestionnaireCompilationVersionService questionnaireCompilationVersionService)
        {
            this.logger = logger;
            this.expressionProcessorGenerator = expressionProcessorGenerator;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.engineVersionService = engineVersionService;
            this.questionnaireCompilationVersionService = questionnaireCompilationVersionService;
        }

        [HttpGet]
        public IActionResult GetAllClassesForLatestVersion(Guid id, int? version)
        {
            var questionnaire = this.GetQuestionnaire(id).Source;

            var supervisorVersion = version ?? this.engineVersionService.LatestSupportedVersion;

            var generated = this.expressionProcessorGenerator.GenerateProcessorStateClasses(questionnaire, supervisorVersion, inSingleFile: true);
            
            var resultBuilder = new StringBuilder();
            
            foreach (KeyValuePair<string, string> keyValuePair in generated)
            {
                resultBuilder.AppendLine(string.Format("//{0}", keyValuePair.Key));
                resultBuilder.AppendLine(keyValuePair.Value);
            }

            return Ok(resultBuilder.ToString());
        }


        [HttpGet]
        public IActionResult GetCompilationResultForLatestVersion(Guid id)
        {
            //do async
            var questionnaire = this.GetQuestionnaire(id).Source;

            var specifiedCompilationVersion = this.questionnaireCompilationVersionService.GetById(id)?.Version;

            var generated = this.expressionProcessorGenerator.GenerateProcessorStateAssembly(questionnaire,
                specifiedCompilationVersion ?? this.engineVersionService.LatestSupportedVersion, out _);
            if (generated.Success)
            {
                return Ok("No errors");
            }
            else
            {
                //var errorLocations = generated.Diagnostics.Select(x => x.Location).Distinct().Aggregate("Errors: \r\n", (current, location) => current + (current + "\r\n" + location));
                var errorLocations = generated.Diagnostics.Select(x => x.Message).ToArray();

                return StatusCode((int) HttpStatusCode.PreconditionFailed, errorLocations);
            }
        }

        [HttpGet]
        public IActionResult GetLatestVersionAssembly(Guid id, int? version)
        {
            //do async
            var supervisorVersion = version ?? this.engineVersionService.LatestSupportedVersion;
            var questionnaire = this.GetQuestionnaire(id).Source;
            string assembly;
            var generated = this.expressionProcessorGenerator.GenerateProcessorStateAssembly(questionnaire, supervisorVersion, out assembly);
            if (generated.Success)
            {
                return File(Convert.FromBase64String(assembly), "application/x-msdownload",
                    string.Format("expressions-{0}.dll", id));
            }
            else
            {
                //var errorLocations = generated.Diagnostics.Select(x => x.Location).Distinct().Aggregate("Errors: \r\n", (current, location) => current + (current + "\r\n" + location));
                var errorLocations = generated.Diagnostics.Select(x => x.Message).Aggregate("Errors: \r\n", (current, message) => current + "\r\n" + message);

                return StatusCode((int) HttpStatusCode.PreconditionFailed, errorLocations);
            }
        }

        private QuestionnaireView GetQuestionnaire(Guid id)
        {
            var questionnaire = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));

            if (questionnaire == null)
            {
                throw new Exception("Questionnaire not found");
            }

            return questionnaire;
        }
    }
}
