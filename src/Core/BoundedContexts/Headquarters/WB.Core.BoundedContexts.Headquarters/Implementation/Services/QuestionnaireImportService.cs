using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Main.Core.Documents;
using Polly;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.Designer;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Questionnaire.Synchronization.Designer;
using WB.Enumerator.Native.Questionnaire;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Questionnaire;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    internal class QuestionnaireImportService : IQuestionnaireImportService
    {
        private readonly ISupportedVersionProvider supportedVersionProvider;
        private readonly IStringCompressor zipUtils;
        private readonly IAttachmentContentService attachmentContentService;
        private readonly IDesignerUserCredentials designerUserCredentials;
        private readonly IDesignerApiFactory designerApiFactory;
        private readonly ITaskRunner taskRunner;
        private readonly IPlainKeyValueStorage<QuestionnairePdf> pdfStorage;
        private readonly IReusableCategoriesStorage reusableCategoriesStorage;
        private readonly IQuestionnaireVersionProvider questionnaireVersionProvider;
        private readonly ITranslationManagementService translationManagementService;
        private readonly IPlainKeyValueStorage<QuestionnaireLookupTable> lookupTablesStorage;
        private readonly ICommandService commandService;
        private readonly ILogger logger;
        private readonly ISystemLog auditLog;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAuthorizedUser authorizedUser;

        public QuestionnaireImportService(
            ISupportedVersionProvider supportedVersionProvider,
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
            IPlainKeyValueStorage<QuestionnairePdf> pdfStorage,
            IReusableCategoriesStorage reusableCategoriesStorage,
            IDesignerUserCredentials designerUserCredentials,
            IDesignerApiFactory designerApiFactory,
            ITaskRunner taskRunner)
        {
            this.supportedVersionProvider = supportedVersionProvider;
            this.zipUtils = zipUtils;
            this.attachmentContentService = attachmentContentService;
            this.questionnaireVersionProvider = questionnaireVersionProvider;
            this.translationManagementService = translationManagementService;
            this.lookupTablesStorage = lookupTablesStorage;
            this.commandService = commandService;
            this.logger = logger;
            this.auditLog = auditLog;
            this.unitOfWork = unitOfWork;
            this.authorizedUser = authorizedUser;
            this.pdfStorage = pdfStorage;
            this.reusableCategoriesStorage = reusableCategoriesStorage;
            this.designerUserCredentials = designerUserCredentials;
            this.designerApiFactory = designerApiFactory;
            this.taskRunner = taskRunner;
        }

        private static ConcurrentDictionary<QuestionnaireIdentity, QuestionnaireImportResult> statuses = new ConcurrentDictionary<QuestionnaireIdentity, QuestionnaireImportResult>();

        public QuestionnaireImportResult GetStatus(QuestionnaireIdentity questionnaireId)
        {
            if (statuses.TryGetValue(questionnaireId, out QuestionnaireImportResult status))
                return status;
            return null;
        }

        public Task<QuestionnaireImportResult> Import(Guid questionnaireId, string name, bool isCensusMode,
            string comment, string requestUrl, bool includePdf = true)
        {
            return ImportAndMigrateAssignments(questionnaireId, name, isCensusMode, comment, requestUrl, includePdf,
                false, null);
        }


        public async Task<QuestionnaireImportResult> ImportAndMigrateAssignments(Guid questionnaireId, 
            string name,
            bool isCensusMode,
            string comment, 
            string requestUrl, 
            bool includePdf, 
            bool shouldMigrateAssignments, 
            QuestionnaireIdentity migrateFrom)
        {
            var designerCredentials = designerUserCredentials.Get();
            var designerApi = designerApiFactory.Get(new ScopeDesignerUserCredentials(designerCredentials));

            try
            {
                await designerApi.IsLoggedIn();
            }
            catch
            {
                return new QuestionnaireImportResult
                {
                    Status = QuestionnaireImportStatus.Error,
                    ImportError = ErrorMessages.IncorrectUserNameOrPassword
                };
            }

            var questionnaireVersion = this.questionnaireVersionProvider.GetNextVersion(questionnaireId);
            var questionnaireIdentity = new QuestionnaireIdentity(questionnaireId, questionnaireVersion);
            var questionnaireImportResult = statuses.GetOrAdd(questionnaireIdentity, new QuestionnaireImportResult
            {
                Identity = questionnaireIdentity,
                QuestionnaireId = questionnaireIdentity.ToString(),
                Progress = new Progress(),
                Status = QuestionnaireImportStatus.NotStarted
            });

            if (questionnaireImportResult.Status == QuestionnaireImportStatus.Error)
                questionnaireImportResult.Status = QuestionnaireImportStatus.NotStarted;

            if (questionnaireImportResult.Status != QuestionnaireImportStatus.NotStarted)
                return questionnaireImportResult;

            var bgTask = taskRunner.Run(async () =>
            {
                return await InScopeExecutor.Current.ExecuteAsync(async (serviceLocatorLocal) =>
                {
                    var questionnaireImportService = (QuestionnaireImportService)serviceLocatorLocal.GetInstance<IQuestionnaireImportService>();
                    var result = await questionnaireImportService.ImportImpl(designerApi, questionnaireImportResult, name, isCensusMode, comment, requestUrl, includePdf);
                    MigrateAssignmentsIfNeed(serviceLocatorLocal, shouldMigrateAssignments, migrateFrom, result);
                    return result;
                });
            });

            if (includePdf == false) // assume this is automation request
            {
                await bgTask;
                return GetStatus(questionnaireIdentity);
            }

            return questionnaireImportResult;
        }

        private void MigrateAssignmentsIfNeed(IServiceLocator serviceLocatorLocal, bool shouldMigrateAssignments, QuestionnaireIdentity migrateFrom,
            QuestionnaireImportResult result)
        {
            if (shouldMigrateAssignments && migrateFrom != null)
            {
                var sourceQuestionnaireId = migrateFrom;

                var processId = Guid.NewGuid();
                var upgradeService = serviceLocatorLocal.GetInstance<IAssignmentsUpgradeService>();
                upgradeService.EnqueueUpgrade(processId, authorizedUser.Id, sourceQuestionnaireId, result.Identity);
                result.MigrateAssignmentProcessId = processId;
                result.Status = QuestionnaireImportStatus.MigrateAssignments;
            }
        }

        private async Task<QuestionnaireImportResult> ImportImpl(IDesignerApi designerApi, QuestionnaireImportResult questionnaireImportResult, string name, bool isCensusMode,
            string comment, string requestUrl, bool includePdf = true)
        {
            var questionnaireIdentity = questionnaireImportResult.Identity;
            var questionnaireId = questionnaireIdentity.QuestionnaireId;

            bool shouldRollback = true;
            try
            {
                // prevent 2 concurrent requests from importing
                var query = this.unitOfWork.Session.CreateSQLQuery("lock table plainstore.questionnairedocuments in EXCLUSIVE mode;");
                await query.ExecuteUpdateAsync();
                questionnaireImportResult.Status = QuestionnaireImportStatus.Progress;

                await TriggerPdfRendering(designerApi, questionnaireImportResult.Identity.QuestionnaireId, includePdf);
                questionnaireImportResult.Progress.Current++;

                var minSupported = supportedVersionProvider.GetMinVerstionSupportedByInterviewer();
                var supportedVersion = supportedVersionProvider.GetSupportedQuestionnaireVersion();
                var questionnairePackage = await designerApi.GetQuestionnaire(questionnaireImportResult.Identity.QuestionnaireId, supportedVersion, minSupported);
                QuestionnaireDocument questionnaire = this.zipUtils.DecompressString<QuestionnaireDocument>(questionnairePackage.Questionnaire);

                questionnaireImportResult.Progress.Total = CalculateTotalOperations(questionnaire);
                questionnaireImportResult.Progress.Current += 2;

                await TriggerPdfTranslationsRendering(designerApi, questionnaireImportResult.Progress, questionnaire, includePdf);

                if (questionnaire.Attachments != null)
                {
                    foreach (var questionnaireAttachment in questionnaire.Attachments)
                    {
                        if (attachmentContentService.HasAttachmentContent(questionnaireAttachment.ContentId))
                        {
                            questionnaireImportResult.Progress.Current += 2;
                            continue;
                        }

                        var attachmentContent = await designerApi.DownloadQuestionnaireAttachment(
                            questionnaireAttachment.ContentId, questionnaireAttachment.AttachmentId);
                        questionnaireImportResult.Progress.Current++;

                        attachmentContentService.SaveAttachmentContent(
                            questionnaireAttachment.ContentId,
                            attachmentContent.ContentType,
                            attachmentContent.FileName,
                            attachmentContent.Content);
                        questionnaireImportResult.Progress.Current++;
                    }
                }

                this.logger.Debug($"checking translations questionnaire {questionnaireId}");
                if (questionnaire.Translations?.Count > 0)
                {
                    translationManagementService.Delete(questionnaireIdentity);

                    this.logger.Debug($"loading translations {questionnaireId}");
                    var translationContent = await designerApi.GetTranslations(questionnaire.PublicKey);
                    questionnaireImportResult.Progress.Current++;

                    translationManagementService.Store(translationContent.Select(x => new TranslationInstance
                    {
                        QuestionnaireId = questionnaireIdentity,
                        Value = x.Value,
                        QuestionnaireEntityId = x.QuestionnaireEntityId,
                        Type = x.Type,
                        TranslationIndex = x.TranslationIndex,
                        TranslationId = x.TranslationId
                    }));
                    questionnaireImportResult.Progress.Current++;
                }

                this.logger.Debug($"checking lookup tables questionnaire {questionnaireId}");
                if (questionnaire.LookupTables.Any())
                {
                    foreach (var lookupId in questionnaire.LookupTables.Keys)
                    {
                        this.logger.Debug($"Loading lookup table questionnaire {questionnaireId}. Lookup id {lookupId}");
                        var lookupTable = await designerApi.GetLookupTables(questionnaire.PublicKey, lookupId);
                        questionnaireImportResult.Progress.Current++;

                        lookupTablesStorage.Store(lookupTable, questionnaireIdentity, lookupId);
                        questionnaireImportResult.Progress.Current++;
                    }
                }

                this.logger.Debug($"checking reusable categories for questionnaire {questionnaireId}");
                if (questionnaire.Categories.Any())
                {
                    foreach (var category in questionnaire.Categories)
                    {
                        this.logger.Debug($"Loading reusable category for questionnaire {questionnaireId}. Category id {category.Id}");
                        var reusableCategories = await designerApi.GetReusableCategories(questionnaire.PublicKey, category.Id);
                        questionnaireImportResult.Progress.Current++;
                        reusableCategoriesStorage.Store(questionnaireIdentity, category.Id, reusableCategories);
                        questionnaireImportResult.Progress.Current++;
                    }
                }

                logger.Verbose($"commandService.Execute.new ImportFromDesigner: {questionnaire.Title}({questionnaire.PublicKey} rev.{questionnaire.Revision})");

                var questionnaireContentVersion = questionnairePackage.QuestionnaireContentVersion;
                var questionnaireAssembly = questionnairePackage.QuestionnaireAssembly;

                commandService.Execute(new ImportFromDesigner(
                    this.authorizedUser.Id,
                    questionnaire,
                    isCensusMode,
                    questionnaireAssembly,
                    questionnaireContentVersion,
                    questionnaireIdentity.Version,
                    comment));
                questionnaireImportResult.Progress.Current++;

                logger.Verbose($"UpdateRevisionMetadata: {questionnaire.Title}({questionnaire.PublicKey} rev.{questionnaire.Revision})");
                await designerApi.UpdateRevisionMetadata(questionnaire.PublicKey, questionnaire.Revision,
                    new QuestionnaireRevisionMetadataModel
                    {
                        HqHost = GetDomainFromUri(requestUrl),
                        HqTimeZoneMinutesOffset =
                            (int)TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes,
                        HqImporterLogin = this.authorizedUser.UserName,
                        HqQuestionnaireVersion = questionnaireIdentity.Version,
                        Comment = comment,
                    });
                questionnaireImportResult.Progress.Current++;


                logger.Verbose($"DownloadAndStorePdf: {questionnaire.Title}({questionnaire.PublicKey} rev.{questionnaire.Revision})");

                await DownloadAndStorePdf(designerApi, questionnaireImportResult.Progress, questionnaireIdentity, questionnaire, includePdf);

                this.auditLog.QuestionnaireImported(questionnaire.Title, questionnaireIdentity);
                questionnaireImportResult.Progress.Current = questionnaireImportResult.Progress.Total;
                questionnaireImportResult.Status = QuestionnaireImportStatus.Finished;

                shouldRollback = false;
                return questionnaireImportResult;
            }
            catch (RestException ex)
            {
                questionnaireImportResult.Status = QuestionnaireImportStatus.Error;

                switch (ex.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                    case HttpStatusCode.Forbidden:
                    case HttpStatusCode.UpgradeRequired:
                    case HttpStatusCode.ExpectationFailed:
                        questionnaireImportResult.ImportError = ex.Message;
                        break;
                    case HttpStatusCode.PreconditionFailed:
                        questionnaireImportResult.ImportError =
                            string.Format(ErrorMessages.Questionnaire_verification_failed, name);
                        break;
                    case HttpStatusCode.NotFound:
                        questionnaireImportResult.ImportError = string.Format(ErrorMessages.TemplateNotFound, name);
                        break;
                    case HttpStatusCode.ServiceUnavailable:
                        questionnaireImportResult.ImportError = ErrorMessages.ServiceUnavailable;
                        break;
                    case HttpStatusCode.RequestTimeout:
                        questionnaireImportResult.ImportError = ErrorMessages.RequestTimeout;
                        break;
                    default:
                        questionnaireImportResult.ImportError = ErrorMessages.ServerError;
                        break;
                }

                return questionnaireImportResult;
            }
            catch (QuestionnaireAssemblyAlreadyExistsException ex)
            {
                questionnaireImportResult.Status = QuestionnaireImportStatus.Error;
                questionnaireImportResult.ImportError = ex.Message;
                this.logger.Error("Failed to import questionnaire from designer", ex);

                return questionnaireImportResult;
            }
            catch (Exception ex)
            {
                questionnaireImportResult.Status = QuestionnaireImportStatus.Error;

                var domainEx = ex.GetSelfOrInnerAs<QuestionnaireException>();
                if (domainEx != null)
                {
                    questionnaireImportResult.ImportError = domainEx.Message;
                    return questionnaireImportResult;
                }

                this.logger.Error($"Designer: error when importing template #{questionnaireId}", ex);

                questionnaireImportResult.ImportError = "Fail to import questionnaire to Headquarters. Please contact support to resolve this problem.";

                throw;
            }
            finally
            {
                if (shouldRollback)
                {
                    questionnaireImportResult.Status = QuestionnaireImportStatus.Error;
                    this.unitOfWork.DiscardChanges();
                }
            }
        }

        private int CalculateTotalOperations(QuestionnaireDocument questionnaire)
        {
            return 3 // pdf rendering and downloading
                   + (questionnaire.Translations?.Count ?? 0) * 3 // translation rendering and downloading
                   + 2 // questionnaire and assembly
                   + questionnaire.Attachments.Count * 2 // download and save attachments
                   + (questionnaire.Translations != null ? 2 : 0) // download and save translations
                   + questionnaire.LookupTables.Count * 2 // lookup tables
                   + questionnaire.Categories.Count * 2 // categories
                   + 1 // run import command
                   + 1 // update designer metadata
                ;
        }

        private async Task TriggerPdfRendering(IDesignerApi designerApi, Guid questionnaireId, bool includePdf)
        {
            if (includePdf)
                await designerApi.GetPdfStatus(questionnaireId);
        }

        private async Task TriggerPdfTranslationsRendering(IDesignerApi designerApi, Progress progress, QuestionnaireDocument questionnaire, bool includePdf)
        {
            if (!includePdf)
                return;

            this.logger.Error($"Requesting pdf generator to start working for questionnaire {questionnaire.PublicKey}");
                        
            foreach (var questionnaireTranslation in questionnaire.Translations)
            {
                await designerApi.GetPdfStatus(questionnaire.PublicKey, questionnaireTranslation.Id);
                progress.Current++;
            }
        }

        private async Task DownloadAndStorePdf(IDesignerApi designerApi, Progress progress, QuestionnaireIdentity questionnaireIdentity,
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
                return await designerApi.GetPdfStatus(questionnaireIdentity.QuestionnaireId);     
            });

            this.logger.Error("Loading pdf for default language");

            var pdfFile = await designerApi.DownloadPdf(questionnaireIdentity.QuestionnaireId);
            progress.Current++;

            this.pdfStorage.Store(new QuestionnairePdf { Content = pdfFile.Content }, questionnaireIdentity.ToString());
            progress.Current++;

            this.logger.Error($"PDF for questionnaire stored {questionnaireIdentity}");

            foreach (var translation in questionnaire.Translations)
            {
                this.logger.Error($"loading pdf for translation {translation}");

                await pdfRetry.ExecuteAsync(async () =>
                {
                    this.logger.Trace($"Waiting for pdf to be ready {questionnaireIdentity}");

                    return await designerApi.GetPdfStatus(questionnaireIdentity.QuestionnaireId, translation.Id);
                });

                var pdfTranslated = await designerApi.DownloadPdf(questionnaireIdentity.QuestionnaireId, translation.Id);
                progress.Current++;

                this.pdfStorage.Store(new QuestionnairePdf { Content = pdfTranslated.Content },
                    $"{translation.Id.FormatGuid()}_{questionnaireIdentity}");
                progress.Current++;

                this.logger.Error($"PDF for questionnaire stored {questionnaireIdentity} translation {translation.Id}, {translation.Name}");
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
