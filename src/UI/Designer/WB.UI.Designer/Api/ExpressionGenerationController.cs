using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using Raven.Abstractions.Logging;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Core.SharedKernels.SurveySolutions.Services;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Designer.Api
{
    [LocalOrDevelopmentAccessOnly]
    public class ExpressionGenerationController : ApiController
    {
        private IExpressionProcessorGenerator expressionProcessorGenerator;
        private readonly ILogger logger;
        private readonly IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory;
        private readonly IQuestionnaireVersionService questionnaireVersionService;

        public ExpressionGenerationController(ILogger logger, IExpressionProcessorGenerator expressionProcessorGenerator, IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory, IQuestionnaireVersionService questionnaireVersionService)
        {
            this.logger = logger;
            this.expressionProcessorGenerator = expressionProcessorGenerator;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.questionnaireVersionService = questionnaireVersionService;
        }

        [HttpGet]
        public HttpResponseMessage GenerateExpressionsClass(Guid id)
        {
            var questionnaire = GetQuestionnaire(id).Source;
            string generated = expressionProcessorGenerator.GenerateProcessorStateSingleClass(questionnaire);

            return Request.CreateResponse(HttpStatusCode.OK, generated);
        }

        [HttpGet]
        public HttpResponseMessage GetAllClasses(Guid id)
        {
            var questionnaire = GetQuestionnaire(id).Source;
            var generated = expressionProcessorGenerator.GenerateProcessorStateClasses(questionnaire);
            
            var resultBuilder =new StringBuilder();
            
            foreach (KeyValuePair<string, string> keyValuePair in generated)
            {
                resultBuilder.AppendLine(string.Format("//{0}", keyValuePair.Key));
                resultBuilder.AppendLine(keyValuePair.Value);
            }

            return Request.CreateResponse(HttpStatusCode.OK, resultBuilder.ToString());
        }

        [HttpGet]
        public HttpResponseMessage GetAssembly(Guid id)
        {
            //do async
            
            var questionnaire = GetQuestionnaire(id).Source;
            string assembly;
            var generated = expressionProcessorGenerator.GenerateProcessorStateAssembly(questionnaire,
                questionnaireVersionService.GetLatestSupportedVersion(), out assembly);
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
                var errorLocations = generated.Diagnostics.Select(x => x.Location).Distinct().Aggregate("Errors: \r\n", (current, location) => current + (current + "\r\n" + location));

                return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, errorLocations);
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
