using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.QuestionnaireCompilationForOldVersions;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.UI.Designer.Code.Attributes;
using WB.UI.Designer1.Extensions;

namespace WB.UI.Designer.Controllers.Api.Tester
{
    [ApiBasicAuth]
    [Route("api/v{version:int}/questionnaires")]
    public class QuestionnairesController : ControllerBase
    {
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly IQuestionnaireVerifier questionnaireVerifier;
        private readonly IExpressionProcessorGenerator expressionProcessorGenerator;
        private readonly IQuestionnaireListViewFactory viewFactory;
        private readonly IDesignerEngineVersionService engineVersionService;
        private readonly IExpressionsPlayOrderProvider expressionsPlayOrderProvider;
        private readonly IQuestionnaireCompilationVersionService questionnaireCompilationVersionService;
        private readonly ISerializer serializer;

        public QuestionnairesController(
            IQuestionnaireViewFactory questionnaireViewFactory,
            IQuestionnaireVerifier questionnaireVerifier,
            IExpressionProcessorGenerator expressionProcessorGenerator,
            IQuestionnaireListViewFactory viewFactory, 
            IDesignerEngineVersionService engineVersionService, 
            IExpressionsPlayOrderProvider expressionsPlayOrderProvider, 
            IQuestionnaireCompilationVersionService questionnaireCompilationVersionService, 
            ISerializer serializer)
        {
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.questionnaireVerifier = questionnaireVerifier;
            this.expressionProcessorGenerator = expressionProcessorGenerator;
            this.viewFactory = viewFactory;
            this.engineVersionService = engineVersionService;
            this.expressionsPlayOrderProvider = expressionsPlayOrderProvider;
            this.questionnaireCompilationVersionService = questionnaireCompilationVersionService;
            this.serializer = serializer;
        }

        [HttpGet]
        [Route("{id:Guid}")]
        public Questionnaire Get(Guid id, int version)
        {
            if(version < ApiVersion.CurrentTesterProtocolVersion)
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.UpgradeRequired));

            var questionnaireView = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));
            if (questionnaireView == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound));
            }

            if (!this.ValidateAccessPermissions(questionnaireView))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }

            if (this.questionnaireVerifier.CheckForErrors(questionnaireView).Any())
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.PreconditionFailed));
            }

            var specifiedCompilationVersion = this.questionnaireCompilationVersionService.GetById(id)?.Version;

            var versionToCompileAssembly = specifiedCompilationVersion ?? Math.Max(20, this.engineVersionService.GetQuestionnaireContentVersion(questionnaireView.Source));

            string resultAssembly;
            try
            {
                GenerationResult generationResult = this.expressionProcessorGenerator.GenerateProcessorStateAssembly(
                    questionnaireView.Source,
                    versionToCompileAssembly, 
                    out resultAssembly);
                if(!generationResult.Success)
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.PreconditionFailed));
            }
            catch (Exception)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.PreconditionFailed));
            }

            var questionnaire = questionnaireView.Source.Clone();
            var readOnlyQuestionnaireDocument = questionnaireView.Source.AsReadOnly();
            questionnaire.ExpressionsPlayOrder = this.expressionsPlayOrderProvider.GetExpressionsPlayOrder(readOnlyQuestionnaireDocument);
            questionnaire.DependencyGraph = this.expressionsPlayOrderProvider.GetDependencyGraph(readOnlyQuestionnaireDocument);
            questionnaire.ValidationDependencyGraph = this.expressionsPlayOrderProvider.GetValidationDependencyGraph(readOnlyQuestionnaireDocument).ToDictionary(x => x.Key, x => x.Value.ToArray());
            questionnaire.Macros = null;
            questionnaire.IsUsingExpressionStorage = versionToCompileAssembly > 19;

            return new Questionnaire
            {
                Document = questionnaire,
                Assembly = resultAssembly
            };
        }

        [HttpGet]
        [Route("")] 
        [ResponseCache(NoStore = true)]
        public IActionResult Get(int version, [FromUri]int pageIndex = 1, [FromUri]int pageSize = 128)
        {
            if (version < ApiVersion.CurrentTesterProtocolVersion)
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.UpgradeRequired));

            var userId = User.GetId();
            var isAdmin = User.IsAdmin();
            var userName = User.GetUserName();
            
            var questionnaireViews = this.viewFactory.GetUserQuestionnaires(userId, isAdmin, pageIndex, pageSize);

            var questionnaires = questionnaireViews.Select(questionnaire => new TesterQuestionnaireListItem
            {
                Id = questionnaire.QuestionnaireId,
                Title = questionnaire.Title,
                LastEntryDate = questionnaire.LastEntryDate,
                Owner = questionnaire.CreatorName,
                IsOwner = questionnaire.CreatorName == userName,
                IsPublic = questionnaire.IsPublic || isAdmin,
                IsShared = questionnaire.SharedPersons.Any(sharedPerson => sharedPerson.UserId == userId)
            });

            return Ok(questionnaires);
        }

        private bool ValidateAccessPermissions(QuestionnaireView questionnaireView)
        {
            if (questionnaireView.IsPublic || questionnaireView.CreatedBy == User.GetId() || this.User.IsAdmin())
                return true;


            return questionnaireView.SharedPersons.Any(x => x.UserId == User.GetId());
        }
    }
}
