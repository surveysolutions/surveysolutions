﻿using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.QuestionnaireCompilationForOldVersions;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.UI.Designer.Code.Attributes;
using WB.UI.Designer.Extensions;

namespace WB.UI.Designer.Controllers.Api.Tester
{
    [AllowOnlyFromWhitelistIP]
    [Authorize]
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

        [HttpGet]
        [Route("{id:Guid}")]
        public IActionResult Get(Guid id, int version)
        {
            if(version < ApiVersion.CurrentTesterProtocolVersion)
                return StatusCode(StatusCodes.Status426UpgradeRequired);

            var questionnaireView = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));
            if (questionnaireView == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }

            if (!this.ValidateAccessPermissions(questionnaireView))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            if (this.questionnaireVerifier.CheckForErrors(questionnaireView).Any())
            {
                return StatusCode(StatusCodes.Status412PreconditionFailed);
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
                    return StatusCode(StatusCodes.Status412PreconditionFailed);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status412PreconditionFailed);
            }

            var questionnaire = questionnaireView.Source.Clone();
            var readOnlyQuestionnaireDocument = questionnaireView.Source.AsReadOnly();
            questionnaire.ExpressionsPlayOrder = this.expressionsPlayOrderProvider.GetExpressionsPlayOrder(readOnlyQuestionnaireDocument);
            questionnaire.DependencyGraph = this.expressionsPlayOrderProvider.GetDependencyGraph(readOnlyQuestionnaireDocument);
            questionnaire.ValidationDependencyGraph = this.expressionsPlayOrderProvider.GetValidationDependencyGraph(readOnlyQuestionnaireDocument).ToDictionary(x => x.Key, x => x.Value.ToArray());
            questionnaire.Macros = null;
            questionnaire.IsUsingExpressionStorage = versionToCompileAssembly > 19;

            var response = this.serializer.Serialize(new Questionnaire
            {
                Document = questionnaire,
                Assembly = resultAssembly
            });

            return Content(response, MediaTypeNames.Application.Json);
        }

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
