using System;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Tasks;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
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
using WB.UI.Headquarters.Models.ComponentModels;
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
        private readonly AssignmentsVerificationTask assignmentsVerificationTask;
        private readonly IAssignmentsImportReader assignmentsImportReader;
        private readonly IPreloadedDataVerifier dataVerifier;
        private readonly IAssignmentsUpgradeService upgradeService;
        private readonly IAllUsersAndQuestionnairesFactory questionnairesFactory;
        private readonly IExportFileNameService exportFileNameService;

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
            AssignmentsVerificationTask assignmentsVerificationTask,
            IAssignmentsImportReader assignmentsImportReader,
            IPreloadedDataVerifier dataVerifier,
            IAssignmentsUpgradeService upgradeService,
            IAllUsersAndQuestionnairesFactory questionnairesFactory, 
            IExportFileNameService exportFileNameService)
            : base(commandService, logger)
        {
            this.preloadingTemplateService = preloadingTemplateService;
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.fileSystemAccessor = fileSystemAccessor;
            this.questionnaireStorage = questionnaireStorage;
            this.assignmentsImportService = assignmentsImportService;
            this.assignmentsVerificationTask = assignmentsVerificationTask;
            this.assignmentsImportReader = assignmentsImportReader;
            this.dataVerifier = dataVerifier;
            this.upgradeService = upgradeService;
            this.questionnairesFactory = questionnairesFactory;
            this.exportFileNameService = exportFileNameService;
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

            if (status?.ProcessStatus == AssignmentsImportProcessStatus.Verification
                || status?.ProcessStatus == AssignmentsImportProcessStatus.Import)
            {
                return RedirectToAction(nameof(InterviewImportIsInProgress), new { questionnaireId = id, version = version });
            }

            SampleUploadView sampleUploadView = this.sampleUploadViewFactory.Load(new SampleUploadViewInputModel(id, version));
            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(new QuestionnaireIdentity(id, version));

            var viewModel = new BatchUploadModel
            {
                QuestionnaireId = id,
                QuestionnaireVersion = version,
                QuestionnaireTitle = questionnaireInfo?.Title,
                FeaturedQuestions = sampleUploadView.IdentifyingQuestions,
                HiddenQuestions = sampleUploadView.HiddenQuestions,
                RosterSizeQuestions = sampleUploadView.RosterSizeQuestions
            };

            return this.View(viewModel);
        }

        [ActivePage(MenuItem.Questionnaires)]
        public ActionResult UpgradeAssignments(Guid id, long version)
        {
            var model = new UpgradeAssignmentsModel();
            model.QuestionnaireIdentity = new QuestionnaireIdentity(id, version);
            model.SurveySetupUrl = Url.Action("Index");
            model.Questionnaires = 
                this.questionnairesFactory.GetOlderQuestionnairesWithPendingAssignments(id, version)
                .Select(x => 
                    new ComboboxOptionModel(new QuestionnaireIdentity(x.TemplateId, x.TemplateVersion).ToString(), 
                                            string.Format(Pages.QuestionnaireNameVersionFirst, x.TemplateName, x.TemplateVersion)))
                .ToList();
            return View(model);
        }

        [ActivePage(MenuItem.Questionnaires)]
        [HttpPost]
        [ActionName("UpgradeAssignments")]
        public ActionResult UpgradeAssignmentsPost(Guid id, long version)
        {
            var processId = Guid.NewGuid();
            var sourceQuestionnaireId = QuestionnaireIdentity.Parse(Request["sourceQuestionnaireId"]);
            this.upgradeService.EnqueueUpgrade(processId, sourceQuestionnaireId, new QuestionnaireIdentity(id, version));
            return RedirectToAction("UpgradeProgress", new {id = processId});
        }

        [ActivePage(MenuItem.Questionnaires)]
        public ActionResult UpgradeProgress(Guid id)
        {
            var progress = this.upgradeService.Status(id);
            if (progress == null)
            {
                return HttpNotFound();
            }

            return View(new
            {
                ProgressUrl = Url.RouteUrl("DefaultApiWithAction", new { httproute = "", controller = "AssignmentsUpgradeApi", action = "Status" }),
                StopUrl = Url.RouteUrl("DefaultApiWithAction", new { httproute = "", controller = "AssignmentsUpgradeApi", action = "Stop" }),
                ExportErrorsUrl = Url.RouteUrl("DefaultApiWithAction", new { httproute = "", controller = "AssignmentsUpgradeApi", action = "ExportErrors" }),
                SurveySetupUrl = Url.Action("Index")
            });
        }

        public ActionResult TemplateDownload(Guid id, long version)
        {
            var pathToFile = this.preloadingTemplateService.GetFilePathToPreloadingTemplate(id, version);
            return this.File(this.fileSystemAccessor.ReadFile(pathToFile), "application/zip", fileDownloadName: this.fileSystemAccessor.GetFileName(pathToFile));
        }

        public ActionResult SimpleTemplateDownload(Guid id, long version)
        {
            var questionnaireIdentity = new QuestionnaireIdentity(id, version);
            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);
            if (questionnaireInfo == null || questionnaireInfo.IsDeleted)
                return this.HttpNotFound();

            string fileName = exportFileNameService.GetFileNameForAssignmentTemplate(questionnaireIdentity);
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

            if (status?.ProcessStatus == AssignmentsImportProcessStatus.Verification
               || status?.ProcessStatus == AssignmentsImportProcessStatus.Import)
            {
                return RedirectToAction(nameof(InterviewImportIsInProgress), new { questionnaireId = model.QuestionnaireId, version = model.QuestionnaireVersion });
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

                PanelImportVerificationError[] fileErrors = this.dataVerifier.VerifyFiles(model.File.FileName, allImportedFileInfos, questionnaire).Take(10).ToArray();
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

                PreloadedFile protectedFile = allImportedFiles.FirstOrDefault(x => x.FileInfo.QuestionnaireOrRosterName
                                                                                             .Equals(ServiceFiles.ProtectedVariables, StringComparison.OrdinalIgnoreCase));
                if (protectedFile != null)
                {
                    var protectedVariablesErrors = this.dataVerifier.VerifyProtectedVariables(
                        model.File.FileName,
                        protectedFile,
                        questionnaire).Take(10).ToArray();

                    if (protectedVariablesErrors.Length > 0)
                    {
                        return this.View("InterviewImportVerificationErrors",
                            CreateError(questionnaireIdentity, model.File.FileName,
                                errors: protectedVariablesErrors));
                    }
                }

                var answerErrors = this.assignmentsImportService
                    .VerifyPanelAndSaveIfNoErrors(model.File.FileName, allImportedFiles.Where(x => !x.Equals(protectedFile)).ToArray(), model.ResponsibleId, protectedFile, questionnaire).Take(10).ToArray();

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

            if (status?.ProcessStatus == AssignmentsImportProcessStatus.Verification
                || status?.ProcessStatus == AssignmentsImportProcessStatus.Import)
            {
                return RedirectToAction(nameof(InterviewImportIsInProgress), new { questionnaireId = model.QuestionnaireId, version = model.QuestionnaireVersion });
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

            if (isFile)
            {
                preloadedFileInfo = this.assignmentsImportReader.ReadTextFileInfo(model.File.InputStream, model.File.FileName);
                preloadedFileInfo.QuestionnaireOrRosterName = questionnaire.VariableName;/*we expect that it is main file*/
            }
            else if (isZip)
            {
                try
                {
                    var preloadedFiles = this.assignmentsImportReader.ReadZipFileInfo(model.File.InputStream);

                    preloadedFileInfo =
                        preloadedFiles.FirstOrDefault(x => x.QuestionnaireOrRosterName == questionnaireFileName) ??
                        preloadedFiles.FirstOrDefault(x => x.QuestionnaireOrRosterName == questionnaire.VariableName) ??
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
                var fileErrors = this.dataVerifier.VerifyFiles(model.File.FileName, new[] {preloadedFileInfo}, questionnaire).Take(10).ToArray();
                if (fileErrors.Any())
                {
                    return this.View("InterviewImportVerificationErrors",
                        CreateError(questionnaireIdentity, model.File.FileName, errors: fileErrors));
                }

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

                var answerErrors = this.assignmentsImportService.VerifySimpleAndSaveIfNoErrors(preloadedFile, model.ResponsibleId, questionnaire).Take(10).ToArray();
                if (answerErrors.Any())
                {
                    return this.View("InterviewImportVerificationErrors",
                        CreateError(questionnaireIdentity, model.File.FileName, errors: answerErrors));
                }

                this.assignmentsVerificationTask.Run(3);
            }
            catch (Exception e)
            {
                this.Logger.Error(@"Import assignments error", e);

                return this.View("InterviewImportVerificationErrors",
                    CreateError(questionnaireIdentity, model.File.FileName, Pages.GlobalSettings_UnhandledExceptionMessage));
            }

            return this.RedirectToAction("InterviewVerificationProgress");
        }

        [ObserverNotAllowed]
        public ActionResult InterviewVerificationProgress()
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;

            var status = this.assignmentsImportService.GetImportStatus();
            if (status == null) return RedirectToAction("Index");

            if (status.ProcessStatus == AssignmentsImportProcessStatus.Import ||
                status.ProcessStatus == AssignmentsImportProcessStatus.ImportCompleted)
                return RedirectToAction(nameof(InterviewImportProgress));

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

            if (status.ProcessStatus == AssignmentsImportProcessStatus.Verification)
                return RedirectToAction(nameof(InterviewVerificationProgress));

            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(status.QuestionnaireIdentity);

            return this.View(new PreloadedDataInterviewProgressModel
            {
                Status = status,
                QuestionnaireId = questionnaireInfo.QuestionnaireId,
                Version = questionnaireInfo.Version,
                QuestionnaireTitle = questionnaireInfo.Title,
            });
        }

        [HttpGet]
        public ActionResult InterviewImportIsInProgress(Guid questionnaireId, long version)
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;
            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(new QuestionnaireIdentity(questionnaireId, version));

            var status = this.assignmentsImportService.GetImportStatus();

            return this.View(new PreloadedDataInProgressModel
            {
                Questionnaire = new PreloadedDataQuestionnaireModel
                {
                    Id = questionnaireId,
                    Version = version,
                    Title = questionnaireInfo?.Title
                },
                ProcessStatus = status?.ProcessStatus
            });
        }

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
