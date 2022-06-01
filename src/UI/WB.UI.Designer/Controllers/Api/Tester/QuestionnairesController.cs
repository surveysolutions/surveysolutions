using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.QuestionnaireCompilationForOldVersions;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.UI.Designer.Controllers.Api.Designer;
using WB.UI.Designer.Controllers.Api.WebTester;
using WB.UI.Designer.Extensions;

namespace WB.UI.Designer.Controllers.Api.Tester
{
    [AuthorizeOrAnonymousQuestionnaire]
    [Route("api/v{version:int}/questionnaires")]
    public class QuestionnairesController : Controller
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

        [QuestionnairePermissions]
        [HttpGet]
        [Route("{id}")]
        public IActionResult Get(QuestionnaireRevision id, int version)
        {
            if(version < ApiVersion.CurrentTesterProtocolVersion)
                return StatusCode(StatusCodes.Status426UpgradeRequired);

            var questionnaireView = this.questionnaireViewFactory.Load(id);
            if (questionnaireView == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }

            var specifiedCompilationVersion = this.questionnaireCompilationVersionService.GetById(id.QuestionnaireId)?.Version;

            var versionToCompileAssembly = specifiedCompilationVersion ?? Math.Max(20, this.engineVersionService.GetQuestionnaireContentVersion(questionnaireView.Source));

            string resultAssembly;
            try
            {
                questionnaireView = new QuestionnaireView(questionnaireView.Source.Clone(), questionnaireView.SharedPersons);
                
                var verificationResult = 
                    this.questionnaireVerifier.CompileAndVerify(questionnaireView,
                      versionToCompileAssembly,
                      id.QuestionnaireId,
                      out resultAssembly);
                
                if (verificationResult.Any(x => x.MessageLevel != VerificationMessageLevel.Warning))
                    return StatusCode(StatusCodes.Status412PreconditionFailed);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status412PreconditionFailed);
            }

            var questionnaire = questionnaireView.Source.Clone();
            var readOnlyQuestionnaireDocument = new ReadOnlyQuestionnaireDocumentWithCache(questionnaireView.Source);
            questionnaire.ExpressionsPlayOrder = this.expressionsPlayOrderProvider.GetExpressionsPlayOrder(readOnlyQuestionnaireDocument);
            questionnaire.DependencyGraph = this.expressionsPlayOrderProvider.GetDependencyGraph(readOnlyQuestionnaireDocument);
            questionnaire.ValidationDependencyGraph = this.expressionsPlayOrderProvider.GetValidationDependencyGraph(readOnlyQuestionnaireDocument).ToDictionary(x => x.Key, x => x.Value.ToArray());
            questionnaire.Macros = new Dictionary<Guid, Macro>();
            questionnaire.IsUsingExpressionStorage = versionToCompileAssembly > 19;

            var response = this.serializer.Serialize(new Questionnaire
            (
                document : questionnaire,
                assembly : resultAssembly
            ));

            return Content(response, MediaTypeNames.Application.Json);
        }

        [Authorize]
        [HttpGet]
        [Route("")] 
        [ResponseCache(NoStore = true)]
        public IActionResult Get(int version, [FromQuery]int pageIndex = 1, [FromQuery]int pageSize = 128)
        {
            if (version < ApiVersion.CurrentTesterProtocolVersion)
                return StatusCode((int) HttpStatusCode.UpgradeRequired);

            var userId = User.GetId();
            var isAdmin = User.IsAdmin();
            var userName = User.GetUserName();
            
            var questionnaireViews = this.viewFactory.GetUserQuestionnaires(userId, isAdmin, pageIndex, pageSize);

            var questionnaires = questionnaireViews.Select(questionnaire => new TesterQuestionnaireListItem
            {
                Id = questionnaire.QuestionnaireId,
                Title = questionnaire.Title,
                LastEntryDate = questionnaire.LastEntryDate,
                Owner = questionnaire.Owner ?? questionnaire.CreatorName,
                IsOwner = (questionnaire.Owner ?? questionnaire.CreatorName) == userName,
                IsPublic = questionnaire.IsPublic || isAdmin,
                IsShared = questionnaire.SharedPersons.Any(sharedPerson => sharedPerson.UserId == userId)
            });

            return Ok(questionnaires);
        }
    }
}
