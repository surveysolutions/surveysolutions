using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.QuestionnaireCompilationForOldVersions;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;

namespace WB.UI.Designer.Controllers.Api.Designer
{
    [Authorize]
    [QuestionnairePermissions]
    public class ExpressionGenerationController : ControllerBase
    {
        private readonly IQuestionnaireVerifier questionnaireVerifier; 
        private readonly IExpressionProcessorGenerator expressionProcessorGenerator;
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly IDesignerEngineVersionService engineVersionService;
        private IQuestionnaireCompilationVersionService questionnaireCompilationVersionService;

        public ExpressionGenerationController(IExpressionProcessorGenerator expressionProcessorGenerator, 
            IQuestionnaireViewFactory questionnaireViewFactory, 
            IDesignerEngineVersionService engineVersionService, 
            IQuestionnaireCompilationVersionService questionnaireCompilationVersionService,
            IQuestionnaireVerifier questionnaireVerifier)
        {
            this.expressionProcessorGenerator = expressionProcessorGenerator;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.engineVersionService = engineVersionService;
            this.questionnaireCompilationVersionService = questionnaireCompilationVersionService;
            this.questionnaireVerifier = questionnaireVerifier;
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
            var questionnaire = this.GetQuestionnaire(id);

            var specifiedCompilationVersion = this.questionnaireCompilationVersionService.GetById(id)?.Version;

            var generated = this.questionnaireVerifier.CompileAndVerify(questionnaire,
                specifiedCompilationVersion ?? this.engineVersionService.LatestSupportedVersion, null, out _);
            if (generated.Any(x => x.MessageLevel > VerificationMessageLevel.Warning))
            {
                //var errorLocations = generated.Diagnostics.Select(x => x.Location).Distinct().Aggregate("Errors: \r\n", (current, location) => current + (current + "\r\n" + location));
                var errorLocations = generated.Select(x => x.Message).ToArray();

                return StatusCode((int) HttpStatusCode.PreconditionFailed, errorLocations);
            }
            else
            {
                return Ok("No errors");
            }
        }

        [HttpGet]
        public IActionResult GetLatestVersionAssembly(Guid id, int? version)
        {
            //do async
            var supervisorVersion = version ?? this.engineVersionService.LatestSupportedVersion;
            var questionnaire = this.GetQuestionnaire(id);
            string assembly;
            var generated = this.questionnaireVerifier.CompileAndVerify(questionnaire, supervisorVersion, null, out assembly);
            if (generated.Any(x => x.MessageLevel > VerificationMessageLevel.Warning))
            {
                //var errorLocations = generated.Diagnostics.Select(x => x.Location).Distinct().Aggregate("Errors: \r\n", (current, location) => current + (current + "\r\n" + location));
                var errorLocations = generated.Select(x => x.Message).Aggregate("Errors: \r\n", (current, message) => current + "\r\n" + message);

                return StatusCode((int) HttpStatusCode.PreconditionFailed, errorLocations);
            }
            else
            {
                return File(Convert.FromBase64String(assembly), "application/x-msdownload",
                    string.Format("expressions-{0}.dll", id));
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
