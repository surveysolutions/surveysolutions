using System;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Tasks;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models;
using WB.UI.Headquarters.Resources;
using messages = WB.Core.BoundedContexts.Headquarters.Resources.PreloadingVerificationMessages;

namespace WB.UI.Headquarters.Controllers
{
    [LimitsFilter]
    [AuthorizeOr403(Roles = "Administrator, Headquarter")]
    public class SurveySetupController : BaseController
    {
        private readonly IPreloadingTemplateService preloadingTemplateService;
        private readonly ISampleUploadViewFactory sampleUploadViewFactory;
        private readonly InterviewDataExportSettings interviewDataExportSettings;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IAssignmentsImportService assignmentsImportService;
        private readonly AssignmentsImportTask assignmentsImportTask;
        private readonly AssignmentsVerificationTask assignmentsVerificationTask;
        private readonly IAssignmentsImportReader assignmentsImportReader;
        private readonly IPreloadedDataVerifier dataVerifier;

        public SurveySetupController(
            ICommandService commandService,
            ILogger logger,
            IPreloadingTemplateService preloadingTemplateService,
            ISampleUploadViewFactory sampleUploadViewFactory,
            InterviewDataExportSettings interviewDataExportSettings,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IFileSystemAccessor fileSystemAccessor,
            IQuestionnaireStorage questionnaireStorage,
            IAssignmentsImportService assignmentsImportService,
            AssignmentsImportTask assignmentsImportTask,
            AssignmentsVerificationTask assignmentsVerificationTask,
            IAssignmentsImportReader assignmentsImportReader,
            IPreloadedDataVerifier dataVerifier)
            : base(commandService, logger)
        {
            this.preloadingTemplateService = preloadingTemplateService;
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.fileSystemAccessor = fileSystemAccessor;
            this.questionnaireStorage = questionnaireStorage;
            this.assignmentsImportService = assignmentsImportService;
            this.assignmentsImportTask = assignmentsImportTask;
            this.assignmentsVerificationTask = assignmentsVerificationTask;
            this.assignmentsImportReader = assignmentsImportReader;
            this.dataVerifier = dataVerifier;
            this.sampleUploadViewFactory = sampleUploadViewFactory;
        }

        public ActionResult Index()
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;
            this.ViewBag.EnableInterviewHistory = this.interviewDataExportSettings.EnableInterviewHistory;
            return this.View();
        }

        public ActionResult BatchUpload(Guid id, long version)
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;

            var status = this.assignmentsImportService.GetImportStatus();

            var prevImportFinishedWithErrors = status != null && status.InQueueCount == status.WithErrorsCount;
            if (!prevImportFinishedWithErrors)
            {
                var assignmentsPageToRedirect = this.GetImportAssignmentsPageToRedirect(status, nameof(BatchUpload));
                if (assignmentsPageToRedirect != null) return assignmentsPageToRedirect;
            }

            var featuredQuestionItems = this.sampleUploadViewFactory.Load(new SampleUploadViewInputModel(id, version)).ColumnListToPreload;
            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(new QuestionnaireIdentity(id, version));

            var viewModel = new BatchUploadModel
            {
                QuestionnaireId = id,
                QuestionnaireVersion = version,
                QuestionnaireTitle = questionnaireInfo?.Title,
                FeaturedQuestions = featuredQuestionItems
            };

            return this.View(viewModel);
        }

        public ActionResult TemplateDownload(Guid id, long version)
        {
            var pathToFile = this.preloadingTemplateService.GetFilePathToPreloadingTemplate(id, version);
            return this.File(this.fileSystemAccessor.ReadFile(pathToFile), "application/zip", fileDownloadName: this.fileSystemAccessor.GetFileName(pathToFile));
        }

        public ActionResult SimpleTemplateDownload(Guid id, long version)
        {
            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(new QuestionnaireIdentity(id, version));
            if (questionnaireInfo == null || questionnaireInfo.IsDeleted)
                return this.HttpNotFound();

            string fileName = this.fileSystemAccessor.MakeValidFileName(questionnaireInfo.Title + ".tab");
            byte[] templateFile = this.preloadingTemplateService.GetPrefilledPreloadingTemplateFile(id, version);
            return this.File(templateFile, "text/tab-separated-values", fileDownloadName: fileName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public ActionResult PanelBatchUploadAndVerify(BatchUploadModel model)
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;

            if (!this.ModelState.IsValid)
                return this.RedirectToAction(nameof(BatchUpload), new { id = model.QuestionnaireId, version = model.QuestionnaireVersion });

            var status = this.assignmentsImportService.GetImportStatus();

            var prevImportFinishedWithErrors = status != null && status.InQueueCount == status.WithErrorsCount;
            if (!prevImportFinishedWithErrors)
            {
                var assignmentsPageToRedirect = this.GetImportAssignmentsPageToRedirect(status, nameof(PanelBatchUploadAndVerify));
                if (assignmentsPageToRedirect != null) return assignmentsPageToRedirect;
            }

            var questionnaireIdentity = new QuestionnaireIdentity(model.QuestionnaireId, model.QuestionnaireVersion);
            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);

            if (questionnaireInfo.IsDeleted)
            {
                return this.View("InterviewImportVerificationErrors",
                    CreateError(questionnaireIdentity, model.File.FileName, global::Resources.BatchUpload.Prerequisite_Questionnaire));
            }

            if (@".zip" != this.fileSystemAccessor.GetFileExtension(model.File.FileName).ToLower())
            {
                return this.View("InterviewImportVerificationErrors",
                    CreateError(questionnaireIdentity, model.File.FileName, global::Resources.BatchUpload.Prerequisite_ZipFile));
            }

            PreloadedFileInfo[] allImportedFileInfos = null;
            try
            {
                allImportedFileInfos = this.assignmentsImportReader.ReadZipFileInfo(model.File.InputStream).ToArray();
            }
            catch (ZipException)
            {
                return this.View("InterviewImportVerificationErrors",
                    CreateError(questionnaireIdentity, model.File.FileName, messages.ArchiveWithPasswordNotSupported));
            }

            if (allImportedFileInfos == null || !allImportedFileInfos.Any())
            {
                return this.View("InterviewImportVerificationErrors",
                    CreateError(questionnaireIdentity, model.File.FileName, messages.PL0024_DataWasNotFound));
            }

            try
            {
                var questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);

                var fileErrors = allImportedFileInfos.SelectMany(x => this.dataVerifier.VerifyFile(x, questionnaire)).Take(10).ToArray();
                if (fileErrors.Any())
                {
                    return this.View("InterviewImportVerificationErrors",
                        CreateError(questionnaireIdentity, model.File.FileName, errors: fileErrors));
                }

                var columnErrors = this.dataVerifier.VerifyColumns(allImportedFileInfos, questionnaire).Take(10).ToArray();
                if (columnErrors.Any())
                {
                    return this.View("InterviewImportVerificationErrors",
                        CreateError(questionnaireIdentity, model.File.FileName, errors: columnErrors));
                }

                var allImportedFiles = this.assignmentsImportReader.ReadZipFile(model.File.InputStream).ToArray();
                var answerErrors = this.assignmentsImportService
                    .VerifyPanel(model.File.FileName, allImportedFiles, questionnaire).Take(10).ToArray();

                if (answerErrors.Any())
                {
                    return this.View("InterviewImportVerificationErrors",
                        CreateError(questionnaireIdentity, model.File.FileName, errors: answerErrors));
                }

                this.assignmentsVerificationTask.Run(3);
            }
            catch (Exception e)
            {
                this.Logger.Error(@"Import panel assignments error", e);

                return this.View("InterviewImportVerificationErrors",
                    CreateError(questionnaireIdentity, model.File.FileName, Pages.GlobalSettings_UnhandledExceptionMessage));
            }

            return this.RedirectToAction("InterviewVerificationProgress");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public ActionResult AssignmentsBatchUploadAndVerify(BatchUploadModel model)
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;

            if (!this.ModelState.IsValid)
                return this.RedirectToAction(nameof(BatchUpload), new { id = model.QuestionnaireId, version = model.QuestionnaireVersion });

            var status = this.assignmentsImportService.GetImportStatus();

            var prevImportFinishedWithErrors = status != null && status.InQueueCount == status.WithErrorsCount;
            if (!prevImportFinishedWithErrors)
            {
                var assignmentsPageToRedirect = this.GetImportAssignmentsPageToRedirect(status, nameof(AssignmentsBatchUploadAndVerify));
                if (assignmentsPageToRedirect != null) return assignmentsPageToRedirect;
            }

            var questionnaireIdentity = new QuestionnaireIdentity(model.QuestionnaireId, model.QuestionnaireVersion);
            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);
            if (questionnaireInfo.IsDeleted)
            {
                return this.View("InterviewImportVerificationErrors",
                    CreateError(questionnaireIdentity, model.File.FileName, global::Resources.BatchUpload.Prerequisite_Questionnaire));
            }

            var questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);
            var questionnaireFileName = this.fileSystemAccessor.MakeStataCompatibleFileName(questionnaireInfo.Title);

            var extension = this.fileSystemAccessor.GetFileExtension(model.File.FileName).ToLower();
            if (!new[] {@".tab", @".txt", @".zip"}.Contains(extension))
            {
                return this.View("InterviewImportVerificationErrors",
                    CreateError(questionnaireIdentity, model.File.FileName, global::Resources.BatchUpload.Prerequisite_TabOrTxtFile));
            }
            
            PreloadedFileInfo preloadedFileInfo = null;

            var isFile = new[] { @".tab", @".txt" }.Contains(extension);
            var isZip = @".zip" == extension;

            if (isFile) preloadedFileInfo = this.assignmentsImportReader.ReadTextFileInfo(model.File.InputStream, model.File.FileName);
            else if (isZip)
            {
                try
                {
                    var preloadedFiles = this.assignmentsImportReader.ReadZipFileInfo(model.File.InputStream);

                    preloadedFileInfo =
                        preloadedFiles.FirstOrDefault(x => x.QuestionnaireOrRosterName == questionnaireFileName) ??
                        preloadedFiles.FirstOrDefault();
                }
                catch (ZipException)
                {
                    return this.View("InterviewImportVerificationErrors",
                        CreateError(questionnaireIdentity, model.File.FileName, messages.ArchiveWithPasswordNotSupported));
                }
            }

            if (preloadedFileInfo == null)
            {
                return this.View("InterviewImportVerificationErrors",
                    CreateError(questionnaireIdentity, model.File.FileName, messages.PL0024_DataWasNotFound));
            }

            try
            {
                var columnErrors = this.dataVerifier.VerifyColumns(new[] { preloadedFileInfo }, questionnaire).Take(10).ToArray();
                if (columnErrors.Any())
                {
                    return this.View("InterviewImportVerificationErrors",
                        CreateError(questionnaireIdentity, model.File.FileName, errors: columnErrors));
                }

                var preloadedFile = isFile
                    ? this.assignmentsImportReader.ReadTextFile(model.File.InputStream, model.File.FileName)
                    : this.assignmentsImportReader.ReadFileFromZip(model.File.InputStream, preloadedFileInfo.FileName);

                if (preloadedFile.Rows.Length == 0)
                {
                    return this.View("InterviewImportVerificationErrors",
                        CreateError(questionnaireIdentity, model.File.FileName, messages.PL0024_DataWasNotFound));
                }

                var answerErrors = this.assignmentsImportService.VerifySimple(preloadedFile, questionnaire).Take(10).ToArray();
                if (answerErrors.Any())
                {
                    return this.View("InterviewImportVerificationErrors",
                        CreateError(questionnaireIdentity, model.File.FileName, errors: answerErrors));
                }

                this.assignmentsImportTask.Run(3);
            }
            catch (Exception e)
            {
                this.Logger.Error(@"Import assignments error", e);

                return this.View("InterviewImportVerificationErrors",
                    CreateError(questionnaireIdentity, model.File.FileName, Pages.GlobalSettings_UnhandledExceptionMessage));
            }

            return this.RedirectToAction("InterviewImportConfirmation");
        }

        [HttpGet]
        public ActionResult InterviewImportConfirmation()
        {
            var status = this.assignmentsImportService.GetImportStatus();
            if (status == null) return RedirectToAction("Index");

            var assignmentsPageToRedirect = this.GetImportAssignmentsPageToRedirect(status, nameof(InterviewImportConfirmation));
            if (assignmentsPageToRedirect != null) return assignmentsPageToRedirect;

            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(status.QuestionnaireIdentity);

            return this.View(new PreloadedDataConfirmationModel
            {
                QuestionnaireId = status.QuestionnaireIdentity.QuestionnaireId,
                Version = status.QuestionnaireIdentity.Version,
                QuestionnaireTitle = questionnaireInfo?.Title,
                FileName = status.FileName,
                EntitiesCount = status.TotalCount,
                WasResponsibleProvided = status.AssignedToInterviewersCount + status.AssignedToSupervisorsCount > 0,
                EnumeratorsCount = status.AssignedToInterviewersCount,
                SupervisorsCount = status.AssignedToSupervisorsCount
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public ActionResult InterviewImportConfirmation(PreloadedDataConfirmationModel model)
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;

            var status = this.assignmentsImportService.GetImportStatus();
            if (status == null) return RedirectToAction("Index");

            var assignmentsPageToRedirect = this.GetImportAssignmentsPageToRedirect(status, nameof(InterviewImportConfirmation));
            if (assignmentsPageToRedirect != null) return assignmentsPageToRedirect;

            var questionnaireIdentity = new QuestionnaireIdentity(model.QuestionnaireId, model.Version);
            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(status.QuestionnaireIdentity);
            if (questionnaireInfo.IsDeleted)
            {
                return this.View("InterviewImportVerificationErrors",
                    CreateError(questionnaireIdentity, null, global::Resources.BatchUpload.Prerequisite_Questionnaire));
            }

            if (!this.ModelState.IsValid)
            {
                model.QuestionnaireTitle = questionnaireInfo.Title;
                return this.View(model);
            }

            if (!model.WasResponsibleProvided && model.ResponsibleId.HasValue)
                this.assignmentsImportService.SetResponsibleToAllImportedAssignments(model.ResponsibleId.Value);

            assignmentsImportTask.Run();

            return this.RedirectToAction("InterviewImportProgress");
        }

        [ObserverNotAllowed]
        public ActionResult InterviewVerificationProgress()
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;

            var status = this.assignmentsImportService.GetImportStatus();
            if (status == null) return RedirectToAction("Index");

            var assignmentsPageToRedirect = this.GetImportAssignmentsPageToRedirect(status, nameof(InterviewVerificationProgress));
            if (assignmentsPageToRedirect != null) return assignmentsPageToRedirect;

            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(status.QuestionnaireIdentity);

            return this.View(new PreloadedDataInterviewProgressModel
            {
                Status = status,
                QuestionnaireId = questionnaireInfo.QuestionnaireId,
                Version = questionnaireInfo.Version,
                QuestionnaireTitle = questionnaireInfo.Title,
            });
        }

        [ObserverNotAllowed]
        public ActionResult InterviewImportProgress()
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;
            
            var status = this.assignmentsImportService.GetImportStatus();
            if (status == null) return RedirectToAction("Index");

            var assignmentsPageToRedirect = this.GetImportAssignmentsPageToRedirect(status, nameof(InterviewImportProgress));
            if (assignmentsPageToRedirect != null) return assignmentsPageToRedirect;

            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(status.QuestionnaireIdentity);

            return this.View(new PreloadedDataInterviewProgressModel
            {
                Status = status,
                QuestionnaireId = questionnaireInfo.QuestionnaireId,
                Version = questionnaireInfo.Version,
                QuestionnaireTitle = questionnaireInfo.Title,
            });
        }

        [ObserverNotAllowed]
        public ActionResult CancelImportAssignments(Guid id, long version)
        {
            this.assignmentsImportService.RemoveAllAssignmentsToImport();
            return this.RedirectToAction(nameof(BatchUpload), new {id, version});
        }

        [Localizable(false)]
        private ActionResult GetImportAssignmentsPageToRedirect(AssignmentsImportStatus status, string actionName)
        {
            if (status == null) return null;

            var importAssignmentsPages = new (string actionName, Func<bool> shouldRedirect)[]
            {
                (nameof(InterviewImportProgress), () => IsAssignmentsImportIsInProgress(status)),
                (nameof(InterviewVerificationProgress), () => IsAssignmentsVerifying(status)),
                (nameof(InterviewImportConfirmation), () => NoResponsibleForVerifiedAssignments(status)),
            };

            foreach (var importAssignmentsPage in importAssignmentsPages)
            {
                if (importAssignmentsPage.shouldRedirect.Invoke() && importAssignmentsPage.actionName != actionName)
                    return RedirectToAction(importAssignmentsPage.actionName);
            }

            return null;
        }

        private static bool NoResponsibleForVerifiedAssignments(AssignmentsImportStatus status)
            => status.VerifiedCount == status.TotalCount &&
               status.InQueueCount > status.WithErrorsCount &&
               status.AssignedToInterviewersCount + status.AssignedToSupervisorsCount == 0;

        private static bool IsAssignmentsVerifying(AssignmentsImportStatus status)
            => status.InQueueCount == status.TotalCount && status.VerifiedCount < status.TotalCount;

        private static bool IsAssignmentsImportIsInProgress(AssignmentsImportStatus status)
            => status.InQueueCount < status.TotalCount && status.InQueueCount > status.WithErrorsCount ||
               status.InQueueCount == status.WithErrorsCount;

        private ImportDataParsingErrorsView CreateError(QuestionnaireIdentity questionnaireIdentity,
            string fileName, string error = null, PanelImportVerificationError[] errors = null)
        {
            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);

            return new ImportDataParsingErrorsView(
                questionnaireIdentity,
                questionnaireInfo?.Title,
                errors ?? new[] {new PanelImportVerificationError(@"PL0000", error)},
                fileName);
        }
    }
}
