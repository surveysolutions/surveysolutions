using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Infrastructure.Native.Threading;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Controllers
{
    [LimitsFilter]
    [Authorize(Roles = "Administrator, Headquarter")]
    public class SurveySetupController : BaseController
    {
        private readonly IPreloadingTemplateService preloadingTemplateService;
        private readonly IPreloadedDataRepository preloadedDataRepository;
        private readonly IPreloadedDataVerifier preloadedDataVerifier;
        private readonly ISampleUploadViewFactory sampleUploadViewFactory;
        private readonly InterviewDataExportSettings interviewDataExportSettings;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IInterviewImportService interviewImportService;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IAuthorizedUser authorizedUser;

        public SurveySetupController(
            ICommandService commandService,
            ILogger logger,
            IPreloadingTemplateService preloadingTemplateService,
            IPreloadedDataRepository preloadedDataRepository,
            IPreloadedDataVerifier preloadedDataVerifier,
            ISampleUploadViewFactory sampleUploadViewFactory,
            InterviewDataExportSettings interviewDataExportSettings,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IInterviewImportService interviewImportService,
            IFileSystemAccessor fileSystemAccessor,
            IAuthorizedUser authorizedUser)
            : base(commandService, logger)
        {
            this.preloadingTemplateService = preloadingTemplateService;
            this.preloadedDataRepository = preloadedDataRepository;
            this.preloadedDataVerifier = preloadedDataVerifier;
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.interviewImportService = interviewImportService;
            this.fileSystemAccessor = fileSystemAccessor;
            this.authorizedUser = authorizedUser;
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
            {
                return this.RedirectToAction(nameof(BatchUpload), new { id = model.QuestionnaireId, version = model.QuestionnaireVersion });
            }

            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(new QuestionnaireIdentity(model.QuestionnaireId, model.QuestionnaireVersion));

            if (questionnaireInfo.IsDeleted)
            {
                return this.View("InterviewImportVerificationErrors",
                    ImportDataParsingErrorsView.CreatePrerequisiteError(
                        model.QuestionnaireId,
                        model.QuestionnaireVersion,
                        questionnaireInfo?.Title,
                        global::Resources.BatchUpload.Prerequisite_Questionnaire,
                        AssignmentImportType.Panel,
                        model.File.FileName));
            }

            if (".zip" != this.fileSystemAccessor.GetFileExtension(model.File.FileName).ToLower())
            {
                return this.View("InterviewImportVerificationErrors",
                    ImportDataParsingErrorsView.CreatePrerequisiteError(
                        model.QuestionnaireId,
                        model.QuestionnaireVersion,
                        questionnaireInfo?.Title,
                        global::Resources.BatchUpload.Prerequisite_ZipFile,
                        AssignmentImportType.Panel,
                        model.File.FileName));
            }

            var preloadedDataId = this.preloadedDataRepository.StorePanelData(model.File.InputStream, model.File.FileName);
            PreloadedContentMetaData preloadedMetadata = this.preloadedDataRepository.GetPreloadedDataMetaInformationForPanelData(preloadedDataId);

            //clean up for security reasons
            if (preloadedMetadata == null)
            {
                this.preloadedDataRepository.DeletePreloadedData(preloadedDataId);

                return this.View("InterviewImportVerificationErrors",
                    ImportDataParsingErrorsView.CreatePrerequisiteError(
                        model.QuestionnaireId,
                        model.QuestionnaireVersion,
                        questionnaireInfo?.Title,
                        global::Resources.BatchUpload.Prerequisite_FileOpen,
                        AssignmentImportType.Panel,
                        model.File.FileName));
            }

            var unsupportedFiles = preloadedMetadata.FilesMetaInformation.Where(file => !file.CanBeHandled);
            if (unsupportedFiles.Any())
            {
                this.preloadedDataRepository.DeletePreloadedData(preloadedDataId);

                return this.View("InterviewImportVerificationErrors",
                    ImportDataParsingErrorsView.CreatePrerequisiteError(
                        model.QuestionnaireId,
                        model.QuestionnaireVersion,
                        questionnaireInfo?.Title,
                        string.Format(
                            global::Resources.BatchUpload.Prerequisite_UnsupportedFiles,
                            string.Join(", ", unsupportedFiles.Select(file => file.FileName))),
                        AssignmentImportType.Panel,
                        model.File.FileName));
            }

            var questionnaireIdentity = new QuestionnaireIdentity(model.QuestionnaireId, model.QuestionnaireVersion);

            var interviewImportProcessId = preloadedMetadata.Id;

            Task.Factory.StartNew(() =>
            {
                ThreadMarkerManager.MarkCurrentThreadAsIsolated();
                try
                {
                    this.interviewImportService.VerifyAssignments(questionnaireIdentity, interviewImportProcessId, model.File.FileName);
                }
                finally
                {
                    ThreadMarkerManager.ReleaseCurrentThreadFromIsolation();
                }
            });

            this.ViewBag.ActivePage = MenuItem.Questionnaires;

            this.interviewImportService.Status.QuestionnaireId = questionnaireIdentity;
            this.interviewImportService.Status.QuestionnaireTitle = questionnaireInfo.Title;



            return this.RedirectToAction("InterviewVerificationProgress", new { id = interviewImportProcessId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public ActionResult AssignmentsBatchUploadAndVerify(BatchUploadModel model)
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;

            if (!this.ModelState.IsValid)
            {
                return this.RedirectToAction("BatchUpload", new { id = model.QuestionnaireId, version = model.QuestionnaireVersion });
            }

            if (this.interviewImportService.Status.IsInProgress)
            {
                return RedirectToAction("InterviewImportIsInProgress", new { questionnaireId = model.QuestionnaireId, version = model.QuestionnaireVersion });
            }

            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(new QuestionnaireIdentity(model.QuestionnaireId, model.QuestionnaireVersion));

            if (questionnaireInfo.IsDeleted)
            {
                return this.View("InterviewImportVerificationErrors",
                    ImportDataParsingErrorsView.CreatePrerequisiteError(
                        model.QuestionnaireId,
                        model.QuestionnaireVersion,
                        questionnaireInfo?.Title,
                        global::Resources.BatchUpload.Prerequisite_Questionnaire,
                        AssignmentImportType.Panel,
                        model.File.FileName));
            }

            var extension = this.fileSystemAccessor.GetFileExtension(model.File.FileName).ToLower();
            if (extension != ".tab" && extension != ".txt" && extension != ".zip")
            {
                return this.View("InterviewImportVerificationErrors",
                    ImportDataParsingErrorsView.CreatePrerequisiteError(
                        model.QuestionnaireId,
                        model.QuestionnaireVersion,
                        questionnaireInfo?.Title,
                        global::Resources.BatchUpload.Prerequisite_TabOrTxtFile,
                        AssignmentImportType.Panel,
                        model.File.FileName));
            }

            var preloadedDataId = this.preloadedDataRepository.StoreSampleData(model.File.InputStream, model.File.FileName);
            var preloadedMetadata = this.preloadedDataRepository.GetPreloadedDataMetaInformationForSampleData(preloadedDataId);
           
            //clean up for security reasons
            if (preloadedMetadata == null)
            {
                this.preloadedDataRepository.DeletePreloadedData(preloadedDataId);
            }

            PreloadedDataByFile preloadedSample = null;
            
            if (preloadedMetadata != null)
            {
                preloadedSample = this.preloadedDataRepository.GetPreloadedDataOfSample(preloadedMetadata.Id);
            }

            // save in future
            var verificationStatus = this.preloadedDataVerifier.VerifyAssignmentsSample(model.QuestionnaireId, model.QuestionnaireVersion, preloadedSample);

            //clean up for security reasons
            if (verificationStatus.Errors.Any())
            {
                if (preloadedSample != null)
                    this.preloadedDataRepository.DeletePreloadedData(preloadedSample.Id);

                return this.View("InterviewImportVerificationErrors", new ImportDataParsingErrorsView(
                    model.QuestionnaireId,
                    model.QuestionnaireVersion,
                    questionnaireInfo?.Title,
                    verificationStatus.Errors.ToArray(),
                    new InterviewImportError[0], 
                    verificationStatus.WasResponsibleProvided,
                    preloadedMetadata?.Id,
                    AssignmentImportType.Assignments,
                    preloadedSample?.FileName));
            }

            this.TempData[$"InterviewImportConfirmation-{preloadedMetadata.Id}"] = new PreloadedDataConfirmationModel
            {
                QuestionnaireId = model.QuestionnaireId,
                Version = model.QuestionnaireVersion,
                QuestionnaireTitle = questionnaireInfo?.Title,
                WasResponsibleProvided = verificationStatus.WasResponsibleProvided,
                Id = preloadedMetadata.Id,
                AssignmentImportType = AssignmentImportType.Assignments,
                FileName = preloadedSample.FileName,
                EnumeratorsCount = verificationStatus.EnumeratorsCount,
                SupervisorsCount = verificationStatus.SupervisorsCount,
                EntitiesCount = verificationStatus.EntitiesCount
            };

            return this.RedirectToAction("InterviewImportConfirmation", new { id = preloadedMetadata.Id, questionnaireId = model.QuestionnaireId, version = model.QuestionnaireVersion });
        }

        [HttpGet]
        public ActionResult InterviewImportVerificationCompleted()
        {
            AssignmentImportStatus status = this.interviewImportService.Status;

            var questionnaireId = status.QuestionnaireId.QuestionnaireId;
            var version = status.QuestionnaireId.Version;

            var title = status.QuestionnaireTitle;

            if (String.IsNullOrWhiteSpace(title))
            {
                var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(new QuestionnaireIdentity(questionnaireId, version));
                title = questionnaireInfo?.Title;
            }

            if (status.IsInProgress)
            {
                return RedirectToAction("InterviewImportIsInProgress", new { questionnaireId = questionnaireId, version = version });
            }

            var verificationState = status.VerificationState;

            var interviewImportProcessId = status.InterviewImportProcessId;

            //clean up for security reasons
            if (verificationState.Errors.Any() || status.State.Errors.Any())
            {
                this.preloadedDataRepository.DeletePreloadedData(interviewImportProcessId);
                return this.View("InterviewImportVerificationErrors", new ImportDataParsingErrorsView(
                    questionnaireId,
                    version,
                    title,
                    verificationState.Errors.ToArray(),
                    status.State.Errors.ToArray(),
                    verificationState.WasResponsibleProvided,
                    interviewImportProcessId,
                    AssignmentImportType.Panel,
                    verificationState.FileName));
            }

            this.TempData[$"InterviewImportConfirmation-{interviewImportProcessId}"] = new PreloadedDataConfirmationModel
            {
                QuestionnaireId = questionnaireId,
                Version = version,
                QuestionnaireTitle = status.QuestionnaireTitle,
                WasResponsibleProvided = verificationState.WasResponsibleProvided,
                Id = interviewImportProcessId,
                AssignmentImportType = AssignmentImportType.Panel,
                FileName = verificationState.FileName,
                EnumeratorsCount = verificationState.EnumeratorsCount,
                SupervisorsCount = verificationState.SupervisorsCount,
                EntitiesCount = verificationState.EntitiesCount
            };

            return RedirectToAction("InterviewImportConfirmation", new { id = interviewImportProcessId, questionnaireId = questionnaireId, version = version });
        }

        [HttpGet]
        public ActionResult InterviewImportIsInProgress(Guid questionnaireId, long version)
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;
            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(new QuestionnaireIdentity(questionnaireId, version));

            return this.View(new PreloadedDataInProgressModel
            {
                Questionnaire = new PreloadedDataQuestionnaireModel
                {
                    Id = questionnaireId,
                    Version = version,
                    Title = questionnaireInfo?.Title
                },
                CurrentProcessId = this.interviewImportService.Status.InterviewImportProcessId,
            });
        }

        [HttpGet]
        public ActionResult InterviewImportConfirmation(string id, Guid questionnaireId, long version)
        {
            if (this.interviewImportService.Status.IsInProgress)
            {
                return RedirectToAction("InterviewImportIsInProgress", new { questionnaireId = questionnaireId, version = version });
            }

            var key = $"InterviewImportConfirmation-{id}";
            PreloadedDataConfirmationModel model = null;
            if (this.TempData.ContainsKey(key))
            {
                model = this.TempData[key] as PreloadedDataConfirmationModel;
            }
            if (model == null)
            {
                // load persisted state in future
                var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(new QuestionnaireIdentity(questionnaireId, version));


                if (questionnaireInfo.IsDeleted)
                {
                    return this.View("InterviewImportVerificationErrors",
                        ImportDataParsingErrorsView.CreatePrerequisiteError(
                            questionnaireId,
                            version,
                            questionnaireInfo?.Title,
                            global::Resources.BatchUpload.Prerequisite_Questionnaire,
                            AssignmentImportType.Panel,
                            null));
                }

                model = new PreloadedDataConfirmationModel
                {
                    Id = id,
                    QuestionnaireId = questionnaireId,
                    Version = version,
                    QuestionnaireTitle = questionnaireInfo?.Title
                };
            }

            return this.View(model);
        }

        [ObserverNotAllowed]
        public ActionResult InterviewVerificationProgress(Guid id)
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;

            AssignmentImportStatus status = this.interviewImportService.Status;

            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(status.QuestionnaireId);

            if (questionnaireInfo == null)
            {
                return RedirectToAction("Index");
            }

            return this.View(new PreloadedDataInterviewProgressModel
            {
                Status = status,
                QuestionnaireId = questionnaireInfo.QuestionnaireId,
                Version = questionnaireInfo.Version,
                QuestionnaireTitle = questionnaireInfo.Title,
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public ActionResult InterviewImportConfirmation(PreloadedDataConfirmationModel model)
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;

            if (this.interviewImportService.Status.IsInProgress)
            {
                return RedirectToAction("InterviewImportIsInProgress", new { questionnaireId = model.QuestionnaireId, version = model.Version });
            }

            var questionnaireIdentity = new QuestionnaireIdentity(model.QuestionnaireId, model.Version);
            QuestionnaireBrowseItem questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);

            if (questionnaireInfo.IsDeleted)
            {
                return this.View("InterviewImportVerificationErrors",
                    ImportDataParsingErrorsView.CreatePrerequisiteError(
                        model.QuestionnaireId,
                        model.Version,
                        questionnaireInfo?.Title,
                        global::Resources.BatchUpload.Prerequisite_Questionnaire,
                        AssignmentImportType.Panel,
                        null));
            }

            this.interviewImportService.Status.QuestionnaireId = questionnaireIdentity;
            this.interviewImportService.Status.QuestionnaireTitle = questionnaireInfo.Title;
            
            if (!this.ModelState.IsValid)
            {
                model.QuestionnaireTitle = questionnaireInfo.Title;
                return this.View(model);
            }

            var headquartersId = this.authorizedUser.Id;

            Task.Factory.StartNew(() =>
            {
                ThreadMarkerManager.MarkCurrentThreadAsIsolated();

                try
                {
                    this.interviewImportService.ImportAssignments(supervisorId: model.ResponsibleId,
                        questionnaireIdentity: questionnaireIdentity, 
                        interviewImportProcessId: model.Id,
                        headquartersId: headquartersId,
                        mode: model.AssignmentImportType,
                        shouldSkipInterviewCreation: questionnaireInfo.AllowAssignments);
                }
                finally
                {
                    this.preloadedDataRepository.DeletePreloadedData(model.Id);

                    ThreadMarkerManager.ReleaseCurrentThreadFromIsolation();
                }
            });

            return this.RedirectToAction("InterviewImportProgress", new { id = model.Id });
        }

        [ObserverNotAllowed]
        public ActionResult InterviewImportProgress(Guid id)
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;

            AssignmentImportStatus status = this.interviewImportService.Status;

            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(status.QuestionnaireId);

            if (questionnaireInfo == null)
            {
                return RedirectToAction("Index");
            }

            return this.View(new PreloadedDataInterviewProgressModel
            {
                Status = status,
                QuestionnaireId = questionnaireInfo.QuestionnaireId,
                Version = questionnaireInfo.Version,
                QuestionnaireTitle = questionnaireInfo.Title,
            });
        }
    }
}