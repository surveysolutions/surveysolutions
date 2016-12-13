using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Translations;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.UI.Headquarters.Resources;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    internal class QuestionnaireImportService : IQuestionnaireImportService
    {
        private readonly ISupportedVersionProvider supportedVersionProvider;
        private readonly IRestService restService;
        private readonly IGlobalInfoProvider globalInfoProvider;
        private readonly string apiPrefix = @"/api/hq";
        private readonly string apiVersion = @"v3";
        private readonly IStringCompressor zipUtils;
        private readonly IAttachmentContentService attachmentContentService;
        private readonly IQuestionnaireVersionProvider questionnaireVersionProvider;
        private readonly ITranslationManagementService translationManagementService;
        private readonly ICommandService commandService;
        private readonly ILogger logger;


        public QuestionnaireImportService(ISupportedVersionProvider supportedVersionProvider, 
            IRestService restService, 
            IGlobalInfoProvider globalInfoProvider, 
            IStringCompressor zipUtils,
            IAttachmentContentService attachmentContentService,
            IQuestionnaireVersionProvider questionnaireVersionProvider,
            ITranslationManagementService translationManagementService,
            ICommandService commandService,
            ILogger logger)
        {
            this.supportedVersionProvider = supportedVersionProvider;
            this.restService = restService;
            this.globalInfoProvider = globalInfoProvider;
            this.zipUtils = zipUtils;
            this.attachmentContentService = attachmentContentService;
            this.questionnaireVersionProvider = questionnaireVersionProvider;
            this.translationManagementService = translationManagementService;
            this.commandService = commandService;
            this.logger = logger;
        }

        public async Task<QuestionnaireImportResult> Import(Guid questionnaireId, string name, bool isCensusMode)
        {
            try
            {
                var supportedVersion = this.supportedVersionProvider.GetSupportedQuestionnaireVersion();

                var designerUserCredentials = this.globalInfoProvider.GetDesignerUserCredentials();
                if (designerUserCredentials == null)
                {
                    return new QuestionnaireImportResult
                    {
                        ImportError = ErrorMessages.IncorrectUserNameOrPassword
                    };
                }

                var questionnairePackage = await this.restService.GetAsync<QuestionnaireCommunicationPackage>(
                    url: $"{this.apiPrefix}/{this.apiVersion}/questionnaires/{questionnaireId}",
                    credentials: designerUserCredentials,
                    queryString: new
                    {
                        clientQuestionnaireContentVersion = supportedVersion,
                        minSupportedQuestionnaireVersion = this.supportedVersionProvider.GetMinVerstionSupportedByInterviewer()
                    });

                QuestionnaireDocument questionnaire =
                    this.zipUtils.DecompressString<QuestionnaireDocument>(questionnairePackage.Questionnaire);
                var questionnaireContentVersion = questionnairePackage.QuestionnaireContentVersion;
                var questionnaireAssembly = questionnairePackage.QuestionnaireAssembly;

                if (questionnaire.Attachments != null)
                {
                    foreach (var questionnaireAttachment in questionnaire.Attachments)
                    {
                        if (this.attachmentContentService.HasAttachmentContent(questionnaireAttachment.ContentId))
                            continue;

                        var attachmentContent = await this.restService.DownloadFileAsync(
                            url: $"{this.apiPrefix}/attachment/{questionnaireAttachment.ContentId}",
                            credentials: designerUserCredentials);

                        this.attachmentContentService.SaveAttachmentContent(questionnaireAttachment.ContentId,
                            attachmentContent.ContentType, attachmentContent.Content);
                    }
                }

                var questionnaireVersion = this.questionnaireVersionProvider.GetNextVersion(questionnaire.PublicKey);
                if (questionnaire.Translations?.Count > 0)
                {
                    var questionnaireIdentity = new QuestionnaireIdentity(questionnaire.PublicKey, questionnaireVersion);

                    this.translationManagementService.Delete(questionnaireIdentity);

                    var translationContent = await this.restService.GetAsync<List<TranslationDto>>(
                        url: $"{this.apiPrefix}/translations/{questionnaire.PublicKey}",
                        credentials: designerUserCredentials);

                    this.translationManagementService.Store(translationContent.Select(x => new TranslationInstance
                    {
                        QuestionnaireId = questionnaireIdentity,
                        Value = x.Value,
                        QuestionnaireEntityId = x.QuestionnaireEntityId,
                        Type = x.Type,
                        TranslationIndex = x.TranslationIndex,
                        TranslationId = x.TranslationId
                    }));

                }
                
                this.commandService.Execute(new ImportFromDesigner(
                    this.globalInfoProvider.GetCurrentUser().Id,
                    questionnaire,
                    isCensusMode,
                    questionnaireAssembly,
                    questionnaireContentVersion,
                    questionnaireVersion));

                return new QuestionnaireImportResult();
            }
            catch (RestException ex)
            {
                var questionnaireVerificationResponse = new QuestionnaireImportResult();

                switch (ex.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                    case HttpStatusCode.Forbidden:
                    case HttpStatusCode.UpgradeRequired:
                    case HttpStatusCode.ExpectationFailed:
                        questionnaireVerificationResponse.ImportError = ex.Message;
                        break;
                    case HttpStatusCode.PreconditionFailed:
                        questionnaireVerificationResponse.ImportError =
                            string.Format(ErrorMessages.Questionnaire_verification_failed, name);
                        break;
                    case HttpStatusCode.NotFound:
                        questionnaireVerificationResponse.ImportError = string.Format(ErrorMessages.TemplateNotFound,
                            name);
                        break;
                    case HttpStatusCode.ServiceUnavailable:
                        questionnaireVerificationResponse.ImportError = ErrorMessages.ServiceUnavailable;
                        break;
                    case HttpStatusCode.RequestTimeout:
                        questionnaireVerificationResponse.ImportError = ErrorMessages.RequestTimeout;
                        break;
                    default:
                        questionnaireVerificationResponse.ImportError = ErrorMessages.ServerError;
                        break;
                }

                return questionnaireVerificationResponse;
            }
            catch (QuestionnaireAssemblyAlreadyExistsException ex)
            {
                var questionnaireVerificationResponse = new QuestionnaireImportResult
                {
                    ImportError = ex.Message
                };
                this.logger.Error("Failed to import questionnaire from designer", ex);

                return questionnaireVerificationResponse;
            }
            catch (Exception ex)
            {
                var domainEx = ex.GetSelfOrInnerAs<QuestionnaireException>();
                if (domainEx != null) return new QuestionnaireImportResult() { ImportError = domainEx.Message };

                this.logger.Error($"Designer: error when importing template #{questionnaireId}", ex);

                throw;
            }
        }
    }
}