using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Tasks;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models;
using WB.UI.Headquarters.Models.ComponentModels;
using WB.UI.Headquarters.Resources;
using messages = WB.Core.BoundedContexts.Headquarters.Resources.PreloadingVerificationMessages;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter")]
    public class SurveySetupController : Controller
    {
        private readonly IPreloadingTemplateService preloadingTemplateService;
        private readonly ISampleUploadViewFactory sampleUploadViewFactory;
        private readonly ILogger<SurveySetupController> logger;
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
        private readonly IAuthorizedUser authorizedUser;

        public SurveySetupController(
            IPreloadingTemplateService preloadingTemplateService,
            ISampleUploadViewFactory sampleUploadViewFactory,
            ILogger<SurveySetupController> logger,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IFileSystemAccessor fileSystemAccessor,
            IQuestionnaireStorage questionnaireStorage,
            IAssignmentsImportService assignmentsImportService,
            AssignmentsVerificationTask assignmentsVerificationTask,
            IAssignmentsImportReader assignmentsImportReader,
            IPreloadedDataVerifier dataVerifier,
            IAssignmentsUpgradeService upgradeService,
            IAllUsersAndQuestionnairesFactory questionnairesFactory, 
            IExportFileNameService exportFileNameService,
            IAuthorizedUser authorizedUser)
        {
            this.preloadingTemplateService = preloadingTemplateService;
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
            this.authorizedUser = authorizedUser;
            this.sampleUploadViewFactory = sampleUploadViewFactory;
            this.logger = logger;
        }

        [ActivePage(MenuItem.Questionnaires)]
        public IActionResult Index()
        {
            var surveySetupModel = new SurveySetupModel();
            surveySetupModel.Title = Dashboard.Questionnaires;
            surveySetupModel.DataUrl = Url.Action("Questionnaires", "QuestionnairesApi");
            surveySetupModel.IsObserver = this.authorizedUser.IsObserving;
            surveySetupModel.IsAdmin = this.authorizedUser.IsAdministrator;
            surveySetupModel.QuestionnaireDetailsUrl = Url.Action("Details", "Questionnaires");
            surveySetupModel.TakeNewInterviewUrl = Url.Action("TakeNew", "HQ");
            surveySetupModel.BatchUploadUrl = Url.Action("BatchUpload", "SurveySetup");
            surveySetupModel.MigrateAssignmentsUrl = Url.Action("UpgradeAssignments", "SurveySetup");
            surveySetupModel.WebInterviewUrl = Url.Action("Settings", "WebInterviewSetup");
            surveySetupModel.DownloadLinksUrl = Url.Action("Download", "LinksExport");
            surveySetupModel.CloneQuestionnaireUrl = Url.Action("CloneQuestionnaire", "HQ");
            surveySetupModel.ExportQuestionnaireUrl = Url.Action("ExportQuestionnaire", "HQ");
            surveySetupModel.SendInvitationsUrl = Url.Action("SendInvitations", "WebInterviewSetup");
            surveySetupModel.ImportQuestionnaireUrl = Url.Action("Import", "Template");

            return this.View(surveySetupModel);
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
        public IActionResult UpgradeAssignments(Guid id, long version)
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
        public IActionResult UpgradeAssignmentsPost(Guid id, long version)
        {
            var processId = Guid.NewGuid();
            var sourceQuestionnaireId = QuestionnaireIdentity.Parse(Request.Form["sourceQuestionnaireId"]);
            this.upgradeService.EnqueueUpgrade(processId, authorizedUser.Id, sourceQuestionnaireId, new QuestionnaireIdentity(id, version));
            return RedirectToAction("UpgradeProgress", new {id = processId});
        }

        [ActivePage(MenuItem.Questionnaires)]
        public IActionResult UpgradeProgress(Guid id)
        {
            var progress = this.upgradeService.Status(id);
            if (progress == null)
            {
                return NotFound();
            }

            return View(new
            {
                ProgressUrl = Url.Action("Status", "AssignmentsUpgradeApi"),
                StopUrl = Url.Action("Stop", "AssignmentsUpgradeApi"),
                ExportErrorsUrl = Url.Action("ExportErrors", "AssignmentsUpgradeApi"),
                SurveySetupUrl = Url.Action("Index")
            });
        }

        public ActionResult TemplateDownload(Guid id, long version)
        {
            var pathToFile = this.preloadingTemplateService.GetFilePathToPreloadingTemplate(id, version);
            return this.File(this.fileSystemAccessor.ReadFile(pathToFile), "application/zip", fileDownloadName: this.fileSystemAccessor.GetFileName(pathToFile));
        }

        public IActionResult SimpleTemplateDownload(Guid id, long version)
        {
            var questionnaireIdentity = new QuestionnaireIdentity(id, version);
            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);
            if (questionnaireInfo == null || questionnaireInfo.IsDeleted)
                return NotFound();

            string fileName = exportFileNameService.GetFileNameForAssignmentTemplate(questionnaireIdentity);
            byte[] templateFile = this.preloadingTemplateService.GetPrefilledPreloadingTemplateFile(id, version);
            return this.File(templateFile, "text/tab-separated-values", fileDownloadName: fileName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public async Task<ActionResult> PanelBatchUploadAndVerify(BatchUploadModel model, IFormFile formFile)
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;

            if (!this.ModelState.IsValid || formFile == null)
            {
                return this.RedirectToAction(nameof(BatchUpload), new { id = model.QuestionnaireId, version = model.QuestionnaireVersion });
            }

            var status = this.assignmentsImportService.GetImportStatus();

            if (status?.ProcessStatus == AssignmentsImportProcessStatus.Verification
               || status?.ProcessStatus == AssignmentsImportProcessStatus.Import)
            {
                return RedirectToAction(nameof(InterviewImportIsInProgress), new { questionnaireId = model.QuestionnaireId, version = model.QuestionnaireVersion });
            }

            var questionnaireIdentity = new QuestionnaireIdentity(model.QuestionnaireId, model.QuestionnaireVersion);
            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);

            var fileName = formFile.FileName;
            if (questionnaireInfo.IsDeleted)
            {
                return this.View("InterviewImportVerificationErrors",
                    CreateError(questionnaireIdentity, fileName, Resources.BatchUpload.Prerequisite_Questionnaire));
            }

            if (@".zip" != this.fileSystemAccessor.GetFileExtension(fileName).ToLower())
            {
                return this.View("InterviewImportVerificationErrors",
                    CreateError(questionnaireIdentity, fileName, Resources.BatchUpload.Prerequisite_ZipFile));
            }

            PreloadedFileInfo[] allImportedFileInfos = null;
            try
            {
                allImportedFileInfos = this.assignmentsImportReader.ReadZipFileInfo(formFile.OpenReadStream()).ToArray();
            }
            catch (ZipException)
            {
                return this.View("InterviewImportVerificationErrors",
                    CreateError(questionnaireIdentity, fileName, messages.ArchiveWithPasswordNotSupported));
            }

            if (allImportedFileInfos == null || !allImportedFileInfos.Any())
            {
                return this.View("InterviewImportVerificationErrors",
                    CreateError(questionnaireIdentity, fileName, messages.PL0024_DataWasNotFound));
            }

            try
            {
                var questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);

                PanelImportVerificationError[] fileErrors = this.dataVerifier.VerifyFiles(formFile.FileName, allImportedFileInfos, questionnaire).Take(10).ToArray();
                if (fileErrors.Any())
                {
                    return this.View("InterviewImportVerificationErrors",
                        CreateError(questionnaireIdentity, formFile.FileName, errors: fileErrors));
                }

                var columnErrors = this.dataVerifier.VerifyColumns(allImportedFileInfos, questionnaire).Take(10).ToArray();
                if (columnErrors.Any())
                {
                    return this.View("InterviewImportVerificationErrors",
                        CreateError(questionnaireIdentity, formFile.FileName, errors: columnErrors));
                }

                var allImportedFiles = this.assignmentsImportReader.ReadZipFile(formFile.OpenReadStream()).ToArray();

                PreloadedFile protectedFile = allImportedFiles.FirstOrDefault(x => x.FileInfo.QuestionnaireOrRosterName
                                                                                             .Equals(ServiceFiles.ProtectedVariables, StringComparison.OrdinalIgnoreCase));
                if (protectedFile != null)
                {
                    var protectedVariablesErrors = this.dataVerifier.VerifyProtectedVariables(
                        formFile.FileName,
                        protectedFile,
                        questionnaire).Take(10).ToArray();

                    if (protectedVariablesErrors.Length > 0)
                    {
                        return this.View("InterviewImportVerificationErrors",
                            CreateError(questionnaireIdentity, formFile.FileName,
                                errors: protectedVariablesErrors));
                    }
                }

                var answerErrors = this.assignmentsImportService
                    .VerifyPanelAndSaveIfNoErrors(formFile.FileName, allImportedFiles.Where(x => !x.Equals(protectedFile)).ToArray(), model.ResponsibleId, protectedFile, questionnaire).Take(10).ToArray();

                if (answerErrors.Any())
                {
                    return this.View("InterviewImportVerificationErrors",
                        CreateError(questionnaireIdentity, formFile.FileName, errors: answerErrors));
                }

                await this.assignmentsVerificationTask.ScheduleRunAsync(3);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, @"Import panel assignments error");

                return this.View("InterviewImportVerificationErrors",
                    CreateError(questionnaireIdentity, formFile.FileName, Pages.GlobalSettings_UnhandledExceptionMessage));
            }

            return this.RedirectToAction("InterviewVerificationProgress");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public async Task<ActionResult> AssignmentsBatchUploadAndVerify(BatchUploadModel model, IFormFile formFile)
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
            var fileName = formFile.FileName;
            if (questionnaireInfo.IsDeleted)
            {
                return this.View("InterviewImportVerificationErrors",
                    CreateError(questionnaireIdentity, fileName, Resources.BatchUpload.Prerequisite_Questionnaire));
            }

            var questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);
            var questionnaireFileName = this.fileSystemAccessor.MakeStataCompatibleFileName(questionnaireInfo.Title);

            var extension = this.fileSystemAccessor.GetFileExtension(fileName).ToLower();
            if (!new[] {@".tab", @".txt", @".zip"}.Contains(extension))
            {
                return this.View("InterviewImportVerificationErrors",
                    CreateError(questionnaireIdentity, fileName, Resources.BatchUpload.Prerequisite_TabOrTxtFile));
            }
            
            PreloadedFileInfo preloadedFileInfo = null;

            var isFile = new[] { @".tab", @".txt" }.Contains(extension);
            var isZip = @".zip" == extension;

            if (isFile)
            {
                preloadedFileInfo = this.assignmentsImportReader.ReadTextFileInfo(formFile.OpenReadStream(), fileName);
                preloadedFileInfo.QuestionnaireOrRosterName = questionnaire.VariableName ?? questionnaire.Title;/*we expect that it is main file*/
            }
            else if (isZip)
            {
                try
                {
                    var preloadedFiles = this.assignmentsImportReader.ReadZipFileInfo(formFile.OpenReadStream());

                    preloadedFileInfo =
                        preloadedFiles.FirstOrDefault(x => x.QuestionnaireOrRosterName == questionnaireFileName) ??
                        preloadedFiles.FirstOrDefault(x => x.QuestionnaireOrRosterName == questionnaire.VariableName) ??
                        preloadedFiles.FirstOrDefault();
                }
                catch (ZipException)
                {
                    return this.View("InterviewImportVerificationErrors",
                        CreateError(questionnaireIdentity, fileName, messages.ArchiveWithPasswordNotSupported));
                }
            }

            if (preloadedFileInfo == null)
            {
                return this.View("InterviewImportVerificationErrors",
                    CreateError(questionnaireIdentity, fileName, messages.PL0024_DataWasNotFound));
            }

            try
            {
                var fileErrors = this.dataVerifier.VerifyFile(fileName, preloadedFileInfo, questionnaire).Take(10).ToArray();
                if (fileErrors.Any())
                {
                    return this.View("InterviewImportVerificationErrors",
                        CreateError(questionnaireIdentity, fileName, errors: fileErrors));
                }

                var columnErrors = this.dataVerifier.VerifyColumns(new[] { preloadedFileInfo }, questionnaire).Take(10).ToArray();
                if (columnErrors.Any())
                {
                    return this.View("InterviewImportVerificationErrors",
                        CreateError(questionnaireIdentity, fileName, errors: columnErrors));
                }

                var preloadedFile = isFile
                    ? this.assignmentsImportReader.ReadTextFile(formFile.OpenReadStream(), fileName)
                    : this.assignmentsImportReader.ReadFileFromZip(formFile.OpenReadStream(), preloadedFileInfo.FileName);

                if (preloadedFile.Rows.Length == 0)
                {
                    return this.View("InterviewImportVerificationErrors",
                        CreateError(questionnaireIdentity, fileName, messages.PL0024_DataWasNotFound));
                }

                var answerErrors = this.assignmentsImportService.VerifySimpleAndSaveIfNoErrors(preloadedFile, model.ResponsibleId, questionnaire).Take(10).ToArray();
                if (answerErrors.Any())
                {
                    return this.View("InterviewImportVerificationErrors",
                        CreateError(questionnaireIdentity, fileName, errors: answerErrors));
                }

                await this.assignmentsVerificationTask.ScheduleRunAsync(3);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, @"Import assignments error");

                return this.View("InterviewImportVerificationErrors",
                    CreateError(questionnaireIdentity, fileName, Pages.GlobalSettings_UnhandledExceptionMessage));
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
