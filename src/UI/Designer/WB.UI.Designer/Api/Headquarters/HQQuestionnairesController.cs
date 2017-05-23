﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Designer.Api.Attributes;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Api.Headquarters
{
    [ApiBasicAuth(onlyAllowedAddresses: true)]
    [RoutePrefix("api/hq/v3/questionnaires")]
    public class HQQuestionnairesController : ApiController
    {
        private readonly IMembershipUserService userHelper;
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly IQuestionnaireVerifier questionnaireVerifier;
        private readonly IExpressionProcessorGenerator expressionProcessorGenerator;
        private readonly IQuestionnaireListViewFactory viewFactory;
        private readonly IPlainStorageAccessor<QuestionnaireListViewItem> listItemStorage;
        private readonly IDesignerEngineVersionService engineVersionService;
        private readonly ISerializer serializer;
        private readonly IStringCompressor zipUtils;
        private readonly IExpressionsPlayOrderProvider expressionsPlayOrderProvider;

        public HQQuestionnairesController(IMembershipUserService userHelper,
            IQuestionnaireViewFactory questionnaireViewFactory,
            IQuestionnaireVerifier questionnaireVerifier,
            IExpressionProcessorGenerator expressionProcessorGenerator,
            IQuestionnaireListViewFactory viewFactory, 
            IDesignerEngineVersionService engineVersionService,
            ISerializer serializer,
            IStringCompressor zipUtils, 
            IPlainStorageAccessor<QuestionnaireListViewItem> listItemStorage, 
            IExpressionsPlayOrderProvider expressionsPlayOrderProvider)
        {
            this.userHelper = userHelper;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.questionnaireVerifier = questionnaireVerifier;
            this.expressionProcessorGenerator = expressionProcessorGenerator;
            this.viewFactory = viewFactory;
            this.engineVersionService = engineVersionService;
            this.serializer = serializer;
            this.zipUtils = zipUtils;
            this.listItemStorage = listItemStorage;
            this.expressionsPlayOrderProvider = expressionsPlayOrderProvider;
        }

        [HttpGet]
        [Route("")]
        //in next version of API rename filter to smth like SearchFor
        //to comply with Amason firewall
        public HttpResponseMessage Get(string filter = "", string sortOrder = "", [FromUri]int pageIndex = 1, [FromUri]int pageSize = 128)
        {
            var questionnaireListView = this.viewFactory.Load(new QuestionnaireListInputModel
            {
                ViewerId = this.userHelper.WebUser.UserId,
                IsAdminMode = this.userHelper.WebUser.IsAdmin,
                Page = pageIndex,
                PageSize = pageSize,
                Order = sortOrder,
                SearchFor = filter
            });

            var questionnaires = new PagedQuestionnaireCommunicationPackage
            {
                TotalCount = questionnaireListView.TotalCount,
                Items = questionnaireListView.Items.Select(questionnaireListItem => new QuestionnaireListItem
                {
                    Id = questionnaireListItem.PublicId,
                    Title = questionnaireListItem.Title,
                    LastModifiedDate = questionnaireListItem.LastEntryDate,
                    OwnerName = questionnaireListItem.Owner
                }).ToList()
            };

            var response = this.Request.CreateResponse(questionnaires);
            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true
            };
            return response;
        }


        [HttpGet]
        [Route("{id:Guid}")]
        public QuestionnaireCommunicationPackage Get(Guid id, int clientQuestionnaireContentVersion, [FromUri]int? minSupportedQuestionnaireVersion = null)
        {
            var questionnaireView = this.GetQuestionnaireViewOrThrow(id);

            this.CheckInvariantsAndThrowIfInvalid(clientQuestionnaireContentVersion, questionnaireView);

            var questionnaireContentVersion = this.engineVersionService.GetQuestionnaireContentVersion(questionnaireView.Source);

            var resultAssembly = this.GetQuestionnaireAssemblyOrThrow(questionnaireView, Math.Max(questionnaireContentVersion, minSupportedQuestionnaireVersion.GetValueOrDefault()));

            var questionnaire = questionnaireView.Source.Clone();
            questionnaire.Macros = null;
            questionnaire.LookupTables = null;
            questionnaire.IsUsingExpressionStorage = true;
            questionnaire.ExpressionsPlayOrder = this.expressionsPlayOrderProvider.GetExpressionsPlayOrder(questionnaire.AsReadOnly());

            return new QuestionnaireCommunicationPackage
            {
                Questionnaire = this.zipUtils.CompressString(this.serializer.Serialize(questionnaire)), // use binder to serialize to the old namespaces and assembly
                QuestionnaireAssembly = resultAssembly,
                QuestionnaireContentVersion = questionnaireContentVersion
            };
        }

        [HttpGet]
        [Route("info/{id:guid}")]
        public QuestionnaireInfo Info(Guid id)
        {
            var questionnaire = this.GetQuestionnaireViewOrThrow(id);
            var listItem = this.listItemStorage.GetById(id.FormatGuid());

            QuestionnaireInfo result = new QuestionnaireInfo
            {
                Id = questionnaire.PublicKey,
                Name = questionnaire.Title,
                CreatedAt = listItem.CreationDate,
                LastUpdatedAt = listItem.LastEntryDate
            };

            foreach (var questionnaireEntry in questionnaire.Source.Children.TreeToEnumerable(x => x.Children))
            {
                var group = questionnaireEntry as IGroup;
                if (group != null)
                {
                    if (group.GetParent().PublicKey == questionnaire.PublicKey)
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
                    var question = questionnaireEntry as IQuestion;
                    if (question != null)
                    {
                        result.QuestionsCount++;
                        if (!string.IsNullOrEmpty(question.ConditionExpression))
                        {
                            result.QuestionsWithConditionsCount++;
                        }
                    }
                }
            }

            return result;
        }

        private void CheckInvariantsAndThrowIfInvalid(int clientVersion, QuestionnaireView questionnaireView)
        {
            if (!this.engineVersionService.IsClientVersionSupported(clientVersion))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.UpgradeRequired)
                {
                    ReasonPhrase = string.Format(ErrorMessages.OldClientPleaseUpdate, clientVersion)
                });
            }

            var listOfNewFeaturesForClient = this.engineVersionService.GetListOfNewFeaturesForClient(questionnaireView.Source, clientVersion).ToList();
            if (listOfNewFeaturesForClient.Any())
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.ExpectationFailed)
                {
                    ReasonPhrase = string.Format(ErrorMessages.YourQuestionnaire_0_ContainsNewFunctionalityWhichIsNotSupportedByYourInstallationPleaseUpdate,
                            questionnaireView.Title,
                            string.Join(", ", listOfNewFeaturesForClient.Select(featureDescription => $"\"{featureDescription}\"")))
                });
            }

            var questionnaireErrors = this.questionnaireVerifier.CheckForErrors(questionnaireView).ToArray();

            if (questionnaireErrors.Any(x => x.MessageLevel > VerificationMessageLevel.Warning))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.PreconditionFailed)
                {
                    ReasonPhrase = ErrorMessages.Questionnaire_verification_failed
                });
            }
        }

        private QuestionnaireView GetQuestionnaireViewOrThrow(Guid questionnaireId)
        {
            var questionnaireView = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(questionnaireId));
            if (questionnaireView == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    ReasonPhrase = string.Format(ErrorMessages.TemplateNotFound, questionnaireId)
                });
            }

            if (!this.ValidateAccessPermissions(questionnaireView))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = ErrorMessages.NoAccessToQuestionnaire
                });
            }
            return questionnaireView;
        }

        private string GetQuestionnaireAssemblyOrThrow(QuestionnaireView questionnaireView, int questionnaireContentVersion)
        {
            GenerationResult generationResult;
            string resultAssembly;
            try
            {
                generationResult = this.expressionProcessorGenerator.GenerateProcessorStateAssembly(
                        questionnaireView.Source, questionnaireContentVersion, out resultAssembly);
            }
            catch (Exception)
            {
                generationResult = new GenerationResult()
                {
                    Success = false,
                    Diagnostics =
                        new List<GenerationDiagnostic>()
                        {
                            new GenerationDiagnostic("Common verifier error", "unknown",
                                GenerationDiagnosticSeverity.Error)
                        }
                };
                resultAssembly = string.Empty;
            }

            if (!generationResult.Success || String.IsNullOrWhiteSpace(resultAssembly))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.UpgradeRequired)
                {
                    ReasonPhrase =
                        string.Format(
                            ErrorMessages
                                .YourQuestionnaire_0_ContainsNewFunctionalityWhichIsNotSupportedByYourInstallationPleaseUpdate,
                            questionnaireView.Title)
                });
            }
            return resultAssembly;
        }

        private bool ValidateAccessPermissions(QuestionnaireView questionnaireView)
        {
            if (questionnaireView.CreatedBy == this.userHelper.WebUser.UserId)
                return true;

            return questionnaireView.SharedPersons.Any(x => x.UserId == this.userHelper.WebUser.UserId);
        }
    }
}