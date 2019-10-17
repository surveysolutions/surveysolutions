using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Dapper;
using Main.Core.Documents;
using Refit;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.Designer;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Enumerator.Native.Questionnaire;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    internal class QuestionnaireImportService : IQuestionnaireImportService
    {
        private readonly ISupportedVersionProvider supportedVersionProvider;
        private readonly IStringCompressor zipUtils;
        private readonly IAttachmentContentService attachmentContentService;
        private readonly IPlainKeyValueStorage<QuestionnaireLookupTable> lookupTablesStorage;
        private readonly IQuestionnaireVersionProvider questionnaireVersionProvider;
        private readonly ITranslationManagementService translationManagementService;
        private readonly ICommandService commandService;
        private readonly ILogger logger;
        private readonly ISystemLog auditLog;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IDesignerUserCredentials designerUserCredentials;
        private readonly IDesignerApi designerApi;

        public QuestionnaireImportService(ISupportedVersionProvider supportedVersionProvider,
            IStringCompressor zipUtils,
            IAttachmentContentService attachmentContentService,
            IQuestionnaireVersionProvider questionnaireVersionProvider,
            ITranslationManagementService translationManagementService,
            IPlainKeyValueStorage<QuestionnaireLookupTable> lookupTablesStorage,
            ICommandService commandService,
            ILogger logger,
            ISystemLog auditLog,
            IUnitOfWork unitOfWork,
            IAuthorizedUser authorizedUser,
            IDesignerUserCredentials designerUserCredentials,
            IDesignerApi designerApi)
        {
            this.supportedVersionProvider = supportedVersionProvider;
            this.zipUtils = zipUtils;
            this.attachmentContentService = attachmentContentService;
            this.questionnaireVersionProvider = questionnaireVersionProvider;
            this.translationManagementService = translationManagementService;
            this.commandService = commandService;
            this.logger = logger;
            this.auditLog = auditLog;
            this.unitOfWork = unitOfWork;
            this.authorizedUser = authorizedUser;
            this.designerUserCredentials = designerUserCredentials;
            this.designerApi = designerApi;
            this.lookupTablesStorage = lookupTablesStorage;
        }

        public async Task<QuestionnaireImportResult> Import(Guid questionnaireId, string name, bool isCensusMode,
            string comment, string requestUrl)
        {
            try
            {
                var query = this.unitOfWork.Session.CreateSQLQuery("select pg_advisory_xact_lock(51658156)"); // prevent 2 concurrent requests from importing
                await query.ExecuteUpdateAsync();

                var supportedVersion = this.supportedVersionProvider.GetSupportedQuestionnaireVersion();

                var credentials = this.designerUserCredentials.Get();
                if (credentials == null)
                {
                    return new QuestionnaireImportResult
                    {
                        ImportError = ErrorMessages.IncorrectUserNameOrPassword
                    };
                }

                var minSupported = this.supportedVersionProvider.GetMinVerstionSupportedByInterviewer();

                var questionnairePackage = await this.designerApi.GetQuestionnaire(questionnaireId,
                    supportedVersion, minSupported);

                QuestionnaireDocument questionnaire = this.zipUtils.DecompressString<QuestionnaireDocument>(questionnairePackage.Questionnaire);

                var questionnaireContentVersion = questionnairePackage.QuestionnaireContentVersion;
                var questionnaireAssembly = questionnairePackage.QuestionnaireAssembly;

                if (questionnaire.Attachments != null)
                {
                    foreach (var questionnaireAttachment in questionnaire.Attachments)
                    {
                        if (this.attachmentContentService.HasAttachmentContent(questionnaireAttachment.ContentId))
                            continue;

                        var attachmentContent = await this.designerApi.DownloadQuestionnaireAttachment(
                            questionnaireAttachment.ContentId, questionnaireAttachment.AttachmentId);
                        
                        this.attachmentContentService.SaveAttachmentContent(
                            questionnaireAttachment.ContentId,
                            attachmentContent.ContentType, 
                            attachmentContent.FileName, 
                            attachmentContent.Content);
                    }
                }

                var questionnaireVersion = this.questionnaireVersionProvider.GetNextVersion(questionnaire.PublicKey);

                var questionnaireIdentity = new QuestionnaireIdentity(questionnaire.PublicKey, questionnaireVersion);
                if (questionnaire.Translations?.Count > 0)
                {
                    this.translationManagementService.Delete(questionnaireIdentity);


                    var translationContent = await this.designerApi.GetTranslations(questionnaire.PublicKey);

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

                if (questionnaire.LookupTables.Any())
                {
                    foreach (var lookupId in questionnaire.LookupTables.Keys)
                    {
                        var lookupTable = await this.designerApi.GetLookupTables(questionnaire.PublicKey, lookupId);

                        lookupTablesStorage.Store(lookupTable, questionnaireIdentity, lookupId);
                    }
                }

                this.commandService.Execute(new ImportFromDesigner(
                    this.authorizedUser.Id,
                    questionnaire,
                    isCensusMode,
                    questionnaireAssembly,
                    questionnaireContentVersion,
                    questionnaireVersion,
                    comment));

                await designerApi.Tag(questionnaire.PublicKey, questionnaire.Revision, new QuestionnaireRevisionMetadataModel
                {
                    HqHost = GetDomainFromUri(requestUrl),
                    HqTimeZoneMinutesOffset = (int)TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes,
                    HqImporterLogin = this.authorizedUser.UserName,
                    HqQuestionnaireVersion = questionnaireIdentity.Version,
                    Comment = "",
                });

                this.auditLog.QuestionnaireImported(questionnaire.Title, questionnaireIdentity);

                return new QuestionnaireImportResult()
                {
                    Identity = questionnaireIdentity
                };
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

        private string GetDomainFromUri(string requestUrl)
        {
            if (requestUrl == null) return null;
            var uri = new Uri(requestUrl);
            return uri.Host + (!uri.IsDefaultPort ? ":" + uri.Port : "");
        }
    }
}
