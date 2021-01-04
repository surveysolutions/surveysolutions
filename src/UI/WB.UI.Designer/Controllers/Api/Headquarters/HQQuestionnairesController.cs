using System;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.QuestionnaireCompilationForOldVersions;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Designer.Code;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Controllers.Api.Headquarters
{
    [Route("api/hq/v3/questionnaires")]
    [Authorize]
    public class HQQuestionnairesController : HQControllerBase
    {
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly IQuestionnaireVerifier questionnaireVerifier;
        private readonly IExpressionProcessorGenerator expressionProcessorGenerator;
        private readonly IQuestionnaireListViewFactory viewFactory;
        private readonly DesignerDbContext listItemStorage;
        private readonly IDesignerEngineVersionService engineVersionService;
        private readonly ISerializer serializer;
        private readonly IStringCompressor zipUtils;
        private readonly IExpressionsPlayOrderProvider expressionsPlayOrderProvider;
        private readonly IQuestionnaireCompilationVersionService questionnaireCompilationVersionService;
        private readonly IQuestionnaireHistoryVersionsService questionnaireHistoryVersionsService;
        
        public HQQuestionnairesController(
            IQuestionnaireViewFactory questionnaireViewFactory,
            IQuestionnaireVerifier questionnaireVerifier,
            IExpressionProcessorGenerator expressionProcessorGenerator,
            IQuestionnaireListViewFactory viewFactory,
            IDesignerEngineVersionService engineVersionService,
            ISerializer serializer,
            IStringCompressor zipUtils,
            DesignerDbContext listItemStorage,
            IExpressionsPlayOrderProvider expressionsPlayOrderProvider,
            IQuestionnaireCompilationVersionService questionnaireCompilationVersionService,
            IQuestionnaireHistoryVersionsService questionnaireHistoryVersionsService)
        {
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.questionnaireVerifier = questionnaireVerifier;
            this.expressionProcessorGenerator = expressionProcessorGenerator;
            this.viewFactory = viewFactory;
            this.engineVersionService = engineVersionService;
            this.serializer = serializer;
            this.zipUtils = zipUtils;
            this.listItemStorage = listItemStorage;
            this.expressionsPlayOrderProvider = expressionsPlayOrderProvider;
            this.questionnaireCompilationVersionService = questionnaireCompilationVersionService;
            this.questionnaireHistoryVersionsService = questionnaireHistoryVersionsService;
        }

        [HttpGet]
        [Route("")]
        [ResponseCache(NoStore = true)]
        //in next version of API rename filter to smth like SearchFor
        //to comply with Amazon firewall
        public IActionResult Get(string filter = "", string sortOrder = "", [FromQuery]int pageIndex = 1, [FromQuery]int pageSize = 128)
        {
            var questionnaireListView = this.viewFactory.Load(new QuestionnaireListInputModel
            {
                ViewerId = User.GetId(),
                IsAdminMode = User.IsAdmin(),
                Page = pageIndex,
                PageSize = pageSize,
                Order = sortOrder,
                SearchFor = filter,
                Type = QuestionnairesType.My | QuestionnairesType.Shared
            });

            var questionnaires = new PagedQuestionnaireCommunicationPackage
            {
                TotalCount = questionnaireListView.TotalCount,
                Items = questionnaireListView.Items.Cast<QuestionnaireListViewItem>()
                    .Select(questionnaireListItem => new QuestionnaireListItem
                    {
                        Id = questionnaireListItem.PublicId,
                        Title = questionnaireListItem.Title,
                        LastModifiedDate = questionnaireListItem.LastEntryDate,
                        OwnerName = questionnaireListItem.CreatorName
                    }).ToList()
            };

            return Ok(questionnaires);
        }

        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Get(Guid id, int clientQuestionnaireContentVersion, [FromQuery]int? minSupportedQuestionnaireVersion = null)
        {
            QuestionnaireView? questionnaireView = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));

            if (questionnaireView == null)
            {
                return this.ErrorWithReasonPhraseForHQ(StatusCodes.Status404NotFound, string.Format(ErrorMessages.TemplateNotFound, id));
            }

            if (!this.ValidateAccessPermissions(questionnaireView))
            {
                return this.ErrorWithReasonPhraseForHQ(StatusCodes.Status403Forbidden, ErrorMessages.NoAccessToQuestionnaire);
            }

            if (!this.engineVersionService.IsClientVersionSupported(clientQuestionnaireContentVersion))
            {
                return this.ErrorWithReasonPhraseForHQ(StatusCodes.Status426UpgradeRequired,
                    string.Format(ErrorMessages.OldClientPleaseUpdate, clientQuestionnaireContentVersion));
            }

            var listOfNewFeaturesForClient = this.engineVersionService.GetListOfNewFeaturesForClient(questionnaireView.Source, clientQuestionnaireContentVersion).ToList();
            if (listOfNewFeaturesForClient.Any())
            {
                var reasonPhrase = string.Format(ErrorMessages.YourQuestionnaire_0_ContainsNewFunctionalityWhichIsNotSupportedByYourInstallationPleaseUpdate,
                    questionnaireView.Title,
                    string.Join(", ", listOfNewFeaturesForClient.Select(featureDescription => $"\"{featureDescription}\"")));
                return this.ErrorWithReasonPhraseForHQ(StatusCodes.Status417ExpectationFailed, reasonPhrase);
            }

            var questionnaireErrors = this.questionnaireVerifier.CheckForErrors(questionnaireView).ToArray();

            if (questionnaireErrors.Any(x => x.MessageLevel > VerificationMessageLevel.Warning))
            {
                return this.ErrorWithReasonPhraseForHQ(StatusCodes.Status412PreconditionFailed,
                    ErrorMessages.Questionnaire_verification_failed);
            }

            var specifiedCompilationVersion = this.questionnaireCompilationVersionService.GetById(id)?.Version;
            int versionToCompileAssembly;

            if (specifiedCompilationVersion.HasValue)
                versionToCompileAssembly = specifiedCompilationVersion.Value;
            else
            {
                var questionnaireContentVersion = this.engineVersionService.GetQuestionnaireContentVersion(questionnaireView.Source);

                versionToCompileAssembly = clientQuestionnaireContentVersion > 19
                    ? Math.Max(20, questionnaireContentVersion)
                    : Math.Max(questionnaireContentVersion, minSupportedQuestionnaireVersion.GetValueOrDefault());
            }

            var resultAssembly = this.GetQuestionnaireAssemblyOrThrow(questionnaireView, versionToCompileAssembly);

            if (string.IsNullOrEmpty(resultAssembly))
            {
                var message = string.Format(
                    ErrorMessages
                        .YourQuestionnaire_0_ContainsNewFunctionalityWhichIsNotSupportedByYourInstallationPleaseUpdate,
                    questionnaireView.Title);
                return this.ErrorWithReasonPhraseForHQ(StatusCodes.Status426UpgradeRequired, message);
            }

            var questionnaire = questionnaireView.Source.Clone();

            var userAgent = Request.Headers["User-Agent"].FirstOrDefault();
            questionnaire.Revision = await this.questionnaireHistoryVersionsService
                .TrackQuestionnaireImportAsync(questionnaire, userAgent, User.GetId());

            questionnaire.IsUsingExpressionStorage = versionToCompileAssembly > 19;
            var readOnlyQuestionnaireDocument = questionnaireView.Source.AsReadOnly();
            questionnaire.ExpressionsPlayOrder = this.expressionsPlayOrderProvider.GetExpressionsPlayOrder(readOnlyQuestionnaireDocument);
            questionnaire.DependencyGraph = this.expressionsPlayOrderProvider.GetDependencyGraph(readOnlyQuestionnaireDocument);
            questionnaire.ValidationDependencyGraph = this.expressionsPlayOrderProvider.GetValidationDependencyGraph(readOnlyQuestionnaireDocument).ToDictionary(x => x.Key, x => x.Value.ToArray());

            return Ok(new QuestionnaireCommunicationPackage
            (
                questionnaire : this.zipUtils.CompressString(this.serializer.Serialize(questionnaire)), // use binder to serialize to the old namespaces and assembly
                questionnaireAssembly : resultAssembly,
                questionnaireContentVersion : versionToCompileAssembly
            ));
        }

        [HttpPost]
        [Route("{id:Guid}/revision/{rev:int}/metadata")]
        public async Task<IActionResult> UpdateRevisionMetadata(Guid id, int rev, [FromBody] QuestionnaireRevisionMetaDataUpdate tagData)
        {
            QuestionnaireView? questionnaireView = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));
            if (questionnaireView == null)
            {
                return this.ErrorWithReasonPhraseForHQ(StatusCodes.Status404NotFound, string.Format(ErrorMessages.TemplateNotFound, id));
            }

            if (!this.ValidateAccessPermissions(questionnaireView))
            {
                return this.ErrorWithReasonPhraseForHQ(StatusCodes.Status403Forbidden, ErrorMessages.NoAccessToQuestionnaire);
            }

            await this.questionnaireHistoryVersionsService.UpdateQuestionnaireMetadataAsync(id, rev, tagData);
            return Ok();
        }
        
        [HttpGet]
        [Route("info/{id:guid}")]
        public IActionResult Info(Guid id)
        {
            var questionnaireView = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));
            if (questionnaireView == null)
            {
                return this.ErrorWithReasonPhraseForHQ(StatusCodes.Status404NotFound, string.Format(ErrorMessages.TemplateNotFound, id));
            }

            if (!this.ValidateAccessPermissions(questionnaireView))
            {
                return this.ErrorWithReasonPhraseForHQ(StatusCodes.Status403Forbidden, ErrorMessages.NoAccessToQuestionnaire);
            }

            var questionnaire = questionnaireView;

            var listItem = this.listItemStorage.Questionnaires.Find(id.FormatGuid());

            QuestionnaireInfo result = new QuestionnaireInfo
            {
                Id = questionnaire.PublicKey,
                Name = questionnaire.Title,
                CreatedAt = listItem.CreationDate,
                LastUpdatedAt = listItem.LastEntryDate
            };

            foreach (var questionnaireEntry in questionnaire.Source.Children.TreeToEnumerable(x => x.Children))
            {
                if (questionnaireEntry is IGroup @group)
                {
                    if (group.GetParent()?.PublicKey == questionnaire.PublicKey)
                    {
                        result.ChaptersCount++;
                    }
                    else if (group.IsRoster)
                    {
                        result.RostersCount++;
                    }
                    else
                    {
                        result.GroupsCount++;
                    }
                }
                else
                {
                    if (questionnaireEntry is IQuestion question)
                    {
                        result.QuestionsCount++;
                        if (!string.IsNullOrEmpty(question.ConditionExpression))
                        {
                            result.QuestionsWithConditionsCount++;
                        }
                    }
                }
            }

            return Ok(result);
        }

        private string GetQuestionnaireAssemblyOrThrow(QuestionnaireView questionnaireView, int questionnaireContentVersion)
        {
            string resultAssembly;
            try
            {
                this.expressionProcessorGenerator.GenerateProcessorStateAssembly(
                        questionnaireView.Source, questionnaireContentVersion, out resultAssembly);
            }
            catch (Exception)
            {
                resultAssembly = string.Empty;
            }

            return resultAssembly;
        }
    }
}
