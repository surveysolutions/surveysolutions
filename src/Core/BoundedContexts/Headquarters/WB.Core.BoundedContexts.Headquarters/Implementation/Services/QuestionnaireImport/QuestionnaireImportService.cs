using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.Designer;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    internal class QuestionnaireImportService : IQuestionnaireImportService
    {
        private readonly IStringCompressor zipUtils;
        private readonly IQuestionnaireImportStatuses questionnaireImportStatuses;
        private readonly IAssignmentsUpgradeService assignmentsUpgradeService;
        private readonly ILogger logger;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IArchiveUtils archiveUtils;
        private readonly IDesignerUserCredentials designerUserCredentials;
        private readonly IRootScopeExecutor inScopeExecutor;

        public QuestionnaireImportService(
            IStringCompressor zipUtils,
            ILogger logger,
            IAuthorizedUser authorizedUser,
            IQuestionnaireImportStatuses questionnaireImportStatuses,
            IAssignmentsUpgradeService assignmentsUpgradeService, 
            IArchiveUtils archiveUtils,
            IDesignerUserCredentials designerUserCredentials,
            IRootScopeExecutor inScopeExecutor)
        {
            this.zipUtils = zipUtils;
            this.logger = logger;
            this.authorizedUser = authorizedUser;
            this.questionnaireImportStatuses = questionnaireImportStatuses;
            this.assignmentsUpgradeService = assignmentsUpgradeService;
            this.archiveUtils = archiveUtils;
            this.designerUserCredentials = designerUserCredentials;
            this.inScopeExecutor = inScopeExecutor;
        }

        public QuestionnaireImportResult GetStatus(Guid processId)
        {
            return questionnaireImportStatuses.GetStatus(processId);
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
            var userId = authorizedUser.Id;
            var userName = authorizedUser.UserName;

            var processId = Guid.NewGuid();
            var questionnaireImportResult = questionnaireImportStatuses.StartNew(processId, new QuestionnaireImportResult
            {
                ProcessId = processId,
                ProgressPercent = 0,
                ShouldMigrateAssignments = shouldMigrateAssignments,
                Status = QuestionnaireImportStatus.NotStarted
            });

            if (questionnaireImportResult.Status == QuestionnaireImportStatus.Error)
            {
                questionnaireImportResult.ProgressPercent = 0;
                questionnaireImportResult.Status = QuestionnaireImportStatus.NotStarted;
            }

            if (questionnaireImportResult.Status != QuestionnaireImportStatus.NotStarted)
                return questionnaireImportResult;

            var designerCredentials = designerUserCredentials.Get();

            var bgTask = Task.Run(async () =>
            {
                return await inScopeExecutor.ExecuteAsync(async (serviceLocatorLocal) =>
                {
                    var designerServiceCredentials = serviceLocatorLocal.GetInstance<IDesignerUserCredentials>();

                    try
                    {
                        
                        designerServiceCredentials.SetTaskCredentials(designerCredentials);

                        var questionnaireImportService = (QuestionnaireImportService)serviceLocatorLocal.GetInstance<IQuestionnaireImportService>();
                        var result = await questionnaireImportService.ImportImpl(designerCredentials, serviceLocatorLocal, userId, userName, 
                            questionnaireId, questionnaireImportResult, name, isCensusMode, comment, requestUrl, 
                            shouldMigrateAssignments, migrateFrom, includePdf);
                        return result;
                    }
                    finally
                    {
                        designerServiceCredentials.SetTaskCredentials(null);
                    }
                });
            });
            
            if (includePdf == false) // assume this is automation request
                return await bgTask;

            return questionnaireImportResult;
        }

        private List<IQuestionnaireImportStep> GetImportSteps(QuestionnaireIdentity questionnaireIdentity, 
            QuestionnaireDocument questionnaireDocument, QuestionnaireImportResult importResult, 
            IDesignerApi designerApi, IServiceLocator serviceLocator, bool includePdf)
        {
            var questionnaireImportSteps = new List<IQuestionnaireImportStep>()
            {
                new QuestionnaireBackupImportStep(questionnaireIdentity, questionnaireDocument, designerApi, 
                     serviceLocator, archiveUtils)
            };

            if (includePdf)
            {
                var pdfStorage = serviceLocator.GetInstance<IPlainKeyValueStorage<QuestionnairePdf>>();
                questionnaireImportSteps.Add(new PdfQuestionnaireImportStep(questionnaireIdentity, questionnaireDocument, designerApi, pdfStorage, logger));
            }

            return questionnaireImportSteps;
        }

        private async Task MigrateAssignmentsIfNeed(Guid userId, bool shouldMigrateAssignments, QuestionnaireIdentity migrateFrom, QuestionnaireImportResult result)
        {
            if (shouldMigrateAssignments && migrateFrom != null)
            {
                await assignmentsUpgradeService.EnqueueUpgrade(result.ProcessId, userId, migrateFrom, result.Identity);
            }
        }

        //import should have it's own scope
        //all scope dependent references should come into the method or resolved inside
        private async Task<QuestionnaireImportResult> ImportImpl(RestCredentials credentials,
            IServiceLocator serviceLocator, Guid userId, string userName, Guid questionnaireId, 
            QuestionnaireImportResult questionnaireImportResult, string name, bool isCensusMode,
            string comment, string requestUrl, bool shouldMigrateAssignments, 
            QuestionnaireIdentity migrateFrom, bool includePdf = true)
        {
            var designerApi = serviceLocator.GetInstance<IDesignerApi>();

            try
            {
                await designerApi.IsLoggedIn();
            }
            catch (Exception ex)
            {
                this.logger.Info("Failed to import questionnaire from designer. Need login in it.", ex);
                questionnaireImportResult.Status = QuestionnaireImportStatus.Error;
                questionnaireImportResult.ImportError = ErrorMessages.IncorrectUserNameOrPassword;
                return questionnaireImportResult;
            }

            var unitOfWork = serviceLocator.GetInstance<IUnitOfWork>();
            var commandService = serviceLocator.GetInstance<ICommandService>();
            var questionnaireVersionProvider = serviceLocator.GetInstance<IQuestionnaireVersionProvider>();
            var supportedVersionProvider = serviceLocator.GetInstance<ISupportedVersionProvider>();
            var auditLog = serviceLocator.GetInstance<ISystemLog>();

            bool shouldRollback = true;
            try
            {
                // prevent 2 concurrent requests from importing
                var query = unitOfWork.Session.CreateSQLQuery("select pg_advisory_xact_lock(51658156);");
                await query.ExecuteUpdateAsync();

                var questionnaireVersion = questionnaireVersionProvider.GetNextVersion(questionnaireId);
                var questionnaireIdentity = new QuestionnaireIdentity(questionnaireId, questionnaireVersion);
                questionnaireImportResult.Identity = questionnaireIdentity;

                questionnaireImportResult.Status = QuestionnaireImportStatus.Prepare;

                var minSupported = supportedVersionProvider.GetMinVerstionSupportedByInterviewer();
                var supportedVersion = supportedVersionProvider.GetSupportedQuestionnaireVersion();
                var questionnairePackage = await designerApi.GetQuestionnaire(questionnaireImportResult.Identity.QuestionnaireId, supportedVersion, minSupported);
                QuestionnaireDocument questionnaire = this.zipUtils.DecompressString<QuestionnaireDocument>(questionnairePackage.Questionnaire);

                questionnaireImportResult.Status = QuestionnaireImportStatus.Progress;

                var importSteps = 
                    this.GetImportSteps(questionnaireIdentity, questionnaire, questionnaireImportResult, designerApi, serviceLocator, includePdf)
                        .Where(step => step.IsNeedProcessing())
                        .ToList();

                #region Progress

                int[] globalProgresses = new int[3];
                IProgress<int> questionnaireProgress = new Progress<int>(value => UpdateGlobalProgress(0, 10, value));
                int[] downloadFromDesignerProgresses = new int[importSteps.Count];
                int[] saveDataProgresses = new int[importSteps.Count];

                void UpdateDownloadProgress(int stepIndex, int percentOfStep)
                {
                    downloadFromDesignerProgresses[stepIndex] = percentOfStep;
                    var progress = downloadFromDesignerProgresses.Sum(progressValue => progressValue / downloadFromDesignerProgresses.Length);
                    UpdateGlobalProgress(1, 85, progress);
                }
                void UpdateSaveProgress(int stepIndex, int percentOfStep)
                {
                    saveDataProgresses[stepIndex] = percentOfStep;
                    var progress = saveDataProgresses.Sum(progressValue => progressValue / saveDataProgresses.Length);
                    UpdateGlobalProgress(2, 5, progress);
                }
                void UpdateGlobalProgress(int stepIndex, int percentOfGlobalStep, int value)
                {
                    globalProgresses[stepIndex] = value * percentOfGlobalStep / 100;
                    questionnaireImportResult.ProgressPercent = globalProgresses.Sum();
                }
                #endregion

                questionnaireProgress.Report(50);

                await Task.WhenAll(importSteps.Select((step, index) =>
                {
                    var progress = new Progress<int>(value => UpdateDownloadProgress(index, value) );
                    return Task.Run(async () => await step.DownloadFromDesignerAsync(progress));
                }));

                for (int i = 0; i < importSteps.Count; i++)
                {
                    var index = i;
                    var importStep = importSteps[index];
                    var progress = new Progress<int>(value => UpdateSaveProgress(index, value));
                    importStep.SaveData(progress);
                }

                logger.Verbose($"commandService.Execute.new ImportFromDesigner: {questionnaire.Title}({questionnaire.PublicKey} rev.{questionnaire.Revision})");

                commandService.Execute(new ImportFromDesigner(
                    userId,
                    questionnaire,
                    isCensusMode,
                    questionnairePackage.QuestionnaireAssembly,
                    questionnairePackage.QuestionnaireContentVersion,
                    questionnaireIdentity.Version,
                    comment));
                questionnaireProgress.Report(80);

                logger.Verbose($"UpdateRevisionMetadata: {questionnaire.Title}({questionnaire.PublicKey} rev.{questionnaire.Revision})");
                await designerApi.UpdateRevisionMetadata(questionnaire.PublicKey, questionnaire.Revision,
                    new QuestionnaireRevisionMetadataModel
                    {
                        HqHost = GetDomainFromUri(requestUrl),
                        HqTimeZoneMinutesOffset = (int)TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes,
                        HqImporterLogin = userName,
                        HqQuestionnaireVersion = questionnaireIdentity.Version,
                        Comment = comment,
                    });
                questionnaireProgress.Report(95);

                auditLog.QuestionnaireImported(questionnaire.Title, questionnaireIdentity, userId, userName);
                questionnaireProgress.Report(100);

                questionnaireImportResult.ProgressPercent = 100;

                await MigrateAssignmentsIfNeed(userId, shouldMigrateAssignments, migrateFrom, questionnaireImportResult);

                questionnaireImportResult.Status = QuestionnaireImportStatus.Finished;

                shouldRollback = false;
                return questionnaireImportResult;
            }
            catch (RestException ex)
            {
                this.logger.Info("Failed to import questionnaire from designer. RestException", ex);

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
                this.logger.Error($"Failed to import questionnaire from designer. id: #{questionnaireId}", ex);

                questionnaireImportResult.Status = QuestionnaireImportStatus.Error;

                var domainEx = ex.GetSelfOrInnerAs<QuestionnaireException>();
                if (domainEx != null)
                {
                    questionnaireImportResult.ImportError = domainEx.Message;
                    return questionnaireImportResult;
                }

                questionnaireImportResult.ImportError = "Fail to import questionnaire to Headquarters. Please contact support to resolve this problem.";
                return questionnaireImportResult;
            }
            finally
            {
                if (shouldRollback)
                {
                    questionnaireImportResult.Status = QuestionnaireImportStatus.Error;
                    unitOfWork.DiscardChanges();
                }
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
