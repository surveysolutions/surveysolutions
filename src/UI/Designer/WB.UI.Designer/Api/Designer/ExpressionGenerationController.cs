using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.UI.Designer.Api
{
    [Authorize]
    public class ExpressionGenerationController : ApiController
    {
        private readonly IExpressionProcessorGenerator expressionProcessorGenerator;
        private readonly ILogger logger;
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly IDesignerEngineVersionService engineVersionService;

        public ExpressionGenerationController(ILogger logger, IExpressionProcessorGenerator expressionProcessorGenerator, IQuestionnaireViewFactory questionnaireViewFactory, IDesignerEngineVersionService engineVersionService)
        {
            this.logger = logger;
            this.expressionProcessorGenerator = expressionProcessorGenerator;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.engineVersionService = engineVersionService;
        }

        [HttpGet]
        public HttpResponseMessage GetAllClassesForLatestVersion(Guid id, int? version)
        {
            var questionnaire = this.GetQuestionnaire(id).Source;

            var supervisorVersion = version ?? this.engineVersionService.LatestSupportedVersion;

            var generated = this.expressionProcessorGenerator.GenerateProcessorStateClasses(questionnaire, supervisorVersion);
            
            var resultBuilder =new StringBuilder();
            
            foreach (KeyValuePair<string, string> keyValuePair in generated)
            {
                resultBuilder.AppendLine(string.Format("//{0}", keyValuePair.Key));
                resultBuilder.AppendLine(keyValuePair.Value);
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, resultBuilder.ToString());
        }


        [HttpGet]
        public HttpResponseMessage GetCompilationResultForLatestVersion(Guid id)
        {
            //do async
            var questionnaire = this.GetQuestionnaire(id).Source;
            string assembly;
            var generated = this.expressionProcessorGenerator.GenerateProcessorStateAssembly(questionnaire,
                this.engineVersionService.LatestSupportedVersion, out assembly);
            if (generated.Success)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, "No errors");
            }
            else
            {
                //var errorLocations = generated.Diagnostics.Select(x => x.Location).Distinct().Aggregate("Errors: \r\n", (current, location) => current + (current + "\r\n" + location));
                var errorLocations = generated.Diagnostics.Select(x => x.Message).ToArray();

                return this.Request.CreateResponse(HttpStatusCode.PreconditionFailed, errorLocations);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetLatestVersionAssembly(Guid id, int? version)
        {
            //do async
            var supervisorVersion = version ?? this.engineVersionService.LatestSupportedVersion;
            var questionnaire = this.GetQuestionnaire(id).Source;
            string assembly;
            var generated = this.expressionProcessorGenerator.GenerateProcessorStateAssembly(questionnaire, supervisorVersion, out assembly);
            if (generated.Success)
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new  ByteArrayContent(Convert.FromBase64String(assembly))
                };

                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-msdownload");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment"); 
                response.Content.Headers.ContentDisposition.FileName = string.Format("expressions-{0}.dll", id);
                return response;
            }
            else
            {
                //var errorLocations = generated.Diagnostics.Select(x => x.Location).Distinct().Aggregate("Errors: \r\n", (current, location) => current + (current + "\r\n" + location));
                var errorLocations = generated.Diagnostics.Select(x => x.Message).Aggregate("Errors: \r\n", (current, message) => current + "\r\n" + message);

                return this.Request.CreateResponse(HttpStatusCode.PreconditionFailed, errorLocations);
            }
        }

        private QuestionnaireView GetQuestionnaire(Guid id)
        {
            var questionnaire = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));

            if (questionnaire == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return questionnaire;
        }
    }
}
