using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Main.Core.Documents;
using Polly;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.Designer;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.Questionnaire.Synchronization.Designer;
using WB.Enumerator.Native.Questionnaire;
using WB.Infrastructure.Native.Questionnaire;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    internal class QuestionnaireImportService : IQuestionnaireImportService
    {
        private readonly ISupportedVersionProvider supportedVersionProvider;
        private readonly IStringCompressor zipUtils;
        private readonly IAttachmentContentService attachmentContentService;
        private readonly IReusableCategoriesStorage reusableCategoriesStorage;
        private readonly IPlainKeyValueStorage<QuestionnaireLookupTable> lookupTablesStorage;
        private readonly IPlainKeyValueStorage<QuestionnairePdf> pdfStorage;
        private readonly IQuestionnaireVersionProvider questionnaireVersionProvider;
        private readonly ITranslationManagementService translationManagementService;
        private readonly ICommandService commandService;
        private readonly ILogger logger;
        private readonly ISystemLog auditLog;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAuthorizedUser authorizedUser;
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
            IDesignerApi designerApi,
            IPlainKeyValueStorage<QuestionnairePdf> pdfStorage,
            IReusableCategoriesStorage reusableCategoriesStorage)
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
            this.designerApi = designerApi;
            this.pdfStorage = pdfStorage;
            this.reusableCategoriesStorage = reusableCategoriesStorage;
            this.lookupTablesStorage = lookupTablesStorage;
        }

        public async Task<QuestionnaireImportResult> Import(Guid questionnaireId, string name, bool isCensusMode, string comment, string requestUrl, bool includePdf = true)
        {
            try
            {
                var query = this.unitOfWork.Session.CreateSQLQuery("select pg_advisory_xact_lock(51658156)"); // prevent 2 concurrent requests from importing
                await query.ExecuteUpdateAsync();

                var supportedVersion = this.supportedVersionProvider.GetSupportedQuestionnaireVersion();

                try { await this.designerApi.IsLoggedIn(); }
                catch
                {
                    return new QuestionnaireImportResult
                    {
                        ImportError = ErrorMessages.IncorrectUserNameOrPassword
                    };
                }

                var minSupported = this.supportedVersionProvider.GetMinVerstionSupportedByInterviewer();

                await TriggerPdfRendering(questionnaireId, includePdf);

                var questionnairePackage = await this.designerApi.GetQuestionnaire(questionnaireId, supportedVersion, minSupported);
                                
                QuestionnaireDocument questionnaire = this.zipUtils.DecompressString<QuestionnaireDocument>(questionnairePackage.Questionnaire);

                await TriggerPdfTranslationsRendering(questionnaire, includePdf);

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

                this.logger.Debug($"checking translations questionnaire {questionnaireId}");
                var questionnaireIdentity = new QuestionnaireIdentity(questionnaire.PublicKey, questionnaireVersion);
                if (questionnaire.Translations?.Count > 0)
                {
                    this.translationManagementService.Delete(questionnaireIdentity);

                    this.logger.Debug($"loading translations {questionnaireId}");
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

                this.logger.Debug($"checking lookup tables questionnaire {questionnaireId}");
                if (questionnaire.LookupTables.Any())
                {
                    foreach (var lookupId in questionnaire.LookupTables.Keys)
                    {
                        this.logger.Debug($"Loading lookup table questionnaire {questionnaireId}. Lookup id {lookupId}");
                        var lookupTable = await this.designerApi.GetLookupTables(questionnaire.PublicKey, lookupId);

                        lookupTablesStorage.Store(lookupTable, questionnaireIdentity, lookupId);
                    }
                }

                this.logger.Debug($"checking reusable categories for questionnaire {questionnaireId}");
                if (questionnaire.Categories.Any())
                {
                    foreach (var category in questionnaire.Categories)
                    {
                        this.logger.Debug($"Loading reusable category for questionnaire {questionnaireId}. Category id {category.Id}");
                        var reusableCategories = await this.designerApi.GetReusableCategories(questionnaire.PublicKey, category.Id);
                        reusableCategoriesStorage.Store(questionnaireIdentity, category.Id, reusableCategories);
                    }
                }

                logger.Verbose($"commandService.Execute.new ImportFromDesigner: {questionnaire.Title}({questionnaire.PublicKey} rev.{questionnaire.Revision})");
                this.commandService.Execute(new ImportFromDesigner(
                    this.authorizedUser.Id,
                    questionnaire,
                    isCensusMode,
                    questionnaireAssembly,
                    questionnaireContentVersion,
                    questionnaireVersion,
                    comment));
                
                logger.Verbose($"UpdateRevisionMetadata: {questionnaire.Title}({questionnaire.PublicKey} rev.{questionnaire.Revision})");
                await designerApi.UpdateRevisionMetadata(questionnaire.PublicKey, questionnaire.Revision, new QuestionnaireRevisionMetadataModel
                {
                    HqHost = GetDomainFromUri(requestUrl),
                    HqTimeZoneMinutesOffset = (int)TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes,
                    HqImporterLogin = this.authorizedUser.UserName,
                    HqQuestionnaireVersion = questionnaireIdentity.Version,
                    Comment = comment,
                });

                logger.Verbose($"DownloadAndStorePdf: {questionnaire.Title}({questionnaire.PublicKey} rev.{questionnaire.Revision})");

                await DownloadAndStorePdf(questionnaireIdentity, questionnaire, includePdf);

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

        private async Task TriggerPdfRendering(Guid questionnaireId, bool includePdf)
        {
            if (includePdf)
                await this.designerApi.GetPdfStatus(questionnaireId);
        }

        private async Task TriggerPdfTranslationsRendering(QuestionnaireDocument questionnaire, bool includePdf)
        {
            if (!includePdf)
                return;

            this.logger.Debug($"Requesting pdf generator to start working for questionnaire {questionnaire.PublicKey}");
                        
            foreach (var questionnaireTranslation in questionnaire.Translations)
            {
                await this.designerApi.GetPdfStatus(questionnaire.PublicKey, questionnaireTranslation.Id);
            }
        }

        private async Task DownloadAndStorePdf(QuestionnaireIdentity questionnaireIdentity,
            QuestionnaireDocument questionnaire, bool includePdf)
        {
            if (!includePdf)
                return;

            var pdfRetry = Policy
                .HandleResult<PdfStatus>(x => x.ReadyForDownload == false && x.CanRetry != true)
                .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));

            await pdfRetry.ExecuteAsync(async () =>
            {
                this.logger.Trace($"Waiting for pdf to be ready {questionnaireIdentity}");
                return await this.designerApi.GetPdfStatus(questionnaireIdentity.QuestionnaireId);     
            });

            this.logger.Debug("Loading pdf for default language");

            var pdfFile = await this.designerApi.DownloadPdf(questionnaireIdentity.QuestionnaireId);

            this.pdfStorage.Store(new QuestionnairePdf { Content = pdfFile.Content }, questionnaireIdentity.ToString());

            this.logger.Debug($"PDF for questionnaire stored {questionnaireIdentity}");

            foreach (var translation in questionnaire.Translations)
            {
                this.logger.Debug($"loading pdf for translation {translation}");

                await pdfRetry.ExecuteAsync(async () =>
                {
                    this.logger.Trace($"Waiting for pdf to be ready {questionnaireIdentity}");

                    return await this.designerApi.GetPdfStatus(questionnaireIdentity.QuestionnaireId, translation.Id);
                });

                var pdfTranslated = await this.designerApi.DownloadPdf(questionnaireIdentity.QuestionnaireId, translation.Id);

                this.pdfStorage.Store(new QuestionnairePdf { Content = pdfTranslated.Content },
                    $"{translation.Id.FormatGuid()}_{questionnaireIdentity}");

                this.logger.Debug($"PDF for questionnaire stored {questionnaireIdentity} translation {translation.Id}, {translation.Name}");
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
