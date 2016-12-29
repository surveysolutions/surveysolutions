using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Infrastructure.Native.Threading;
using WB.UI.Headquarters.Models;
using WB.UI.Headquarters.Services;

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

        public SurveySetupController(
            ICommandService commandService,
            IGlobalInfoProvider provider,
            ILogger logger,
            IPreloadingTemplateService preloadingTemplateService,
            IPreloadedDataRepository preloadedDataRepository,
            IPreloadedDataVerifier preloadedDataVerifier,
            ISampleUploadViewFactory sampleUploadViewFactory,
            InterviewDataExportSettings interviewDataExportSettings,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IInterviewImportService interviewImportService,
            IFileSystemAccessor fileSystemAccessor)
            : base(commandService, provider, logger)
        {
            this.preloadingTemplateService = preloadingTemplateService;
            this.preloadedDataRepository = preloadedDataRepository;
            this.preloadedDataVerifier = preloadedDataVerifier;
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.interviewImportService = interviewImportService;
            this.fileSystemAccessor = fileSystemAccessor;
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
                return this.RedirectToAction("BatchUpload", new { id = model.QuestionnaireId, version = model.QuestionnaireVersion });
            }

            var preloadedDataId = this.preloadedDataRepository.Store(model.File.InputStream, model.File.FileName);
            PreloadedContentMetaData preloadedMetadata = this.preloadedDataRepository.GetPreloadedDataMetaInformationForPanelData(preloadedDataId);

            //clean up for security reasons
            if (preloadedMetadata == null)
            {
                this.preloadedDataRepository.DeletePreloadedDataOfSample(preloadedDataId);
            }

            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(new QuestionnaireIdentity(model.QuestionnaireId, model.QuestionnaireVersion));

            PreloadedDataByFile[] preloadedPanelData = this.preloadedDataRepository.GetPreloadedDataOfPanel(preloadedMetadata.Id);

            VerificationStatus verificationStatus = this.preloadedDataVerifier.VerifyPanel(model.QuestionnaireId, model.QuestionnaireVersion, preloadedPanelData);

            //clean up for security reasons
            if (verificationStatus.Errors.Any())
            {
                this.preloadedDataRepository.DeletePreloadedDataOfPanel(preloadedMetadata.Id);
                return this.View("InterviewImportVerificationErrors", new PreloadedDataVerificationErrorsView(
                    model.QuestionnaireId,
                    model.QuestionnaireVersion,
                    questionnaireInfo?.Title,
                    verificationStatus.Errors.ToArray(),
                    verificationStatus.WasResponsibleProvided,
                    preloadedMetadata.Id,
                    PreloadedContentType.Panel,
                    model.File.FileName));
            }

            this.TempData[$"InterviewImportConfirmation-{preloadedMetadata.Id}"] = new PreloadedDataConfirmationModel
            {
                QuestionnaireId = model.QuestionnaireId,
                Version = model.QuestionnaireVersion,
                QuestionnaireTitle = questionnaireInfo?.Title,
                WasSupervsorProvided = verificationStatus.WasResponsibleProvided,
                Id = preloadedMetadata.Id,
                PreloadedContentType = PreloadedContentType.Panel,
                FileName = model.File.FileName,
                EnumeratorsCount = verificationStatus.EnumeratorsCount,
                SupervisorsCount = verificationStatus.SupervisorsCount,
                InterviewsCount = verificationStatus.InterviewsCount
            };

            return RedirectToAction("InterviewImportConfirmation", new { id = preloadedMetadata.Id, questionnaireId = model.QuestionnaireId, version = model.QuestionnaireVersion });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public ActionResult SampleBatchUploadAndVerify(BatchUploadModel model)
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

            var preloadedDataId = this.preloadedDataRepository.Store(model.File.InputStream, model.File.FileName);
            var preloadedMetadata = this.preloadedDataRepository.GetPreloadedDataMetaInformationForSampleData(preloadedDataId);

            //clean up for security reasons
            if (preloadedMetadata == null)
            {
                this.preloadedDataRepository.DeletePreloadedDataOfSample(preloadedDataId);
            }

            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(new QuestionnaireIdentity(model.QuestionnaireId, model.QuestionnaireVersion));

            PreloadedDataByFile preloadedSample = null;

            if (preloadedMetadata != null)
            {
                preloadedSample = this.preloadedDataRepository.GetPreloadedDataOfSample(preloadedMetadata.Id);
            }

            // save in future
            var verificationStatus = this.preloadedDataVerifier.VerifySample(model.QuestionnaireId, model.QuestionnaireVersion, preloadedSample);

            //clean up for security reasons
            if (verificationStatus.Errors.Any())
            {
                if (preloadedSample != null)
                    this.preloadedDataRepository.DeletePreloadedDataOfSample(preloadedSample.Id);

                return this.View("InterviewImportVerificationErrors", new PreloadedDataVerificationErrorsView(
                    model.QuestionnaireId,
                    model.QuestionnaireVersion,
                    questionnaireInfo?.Title,
                    verificationStatus.Errors.ToArray(),
                    verificationStatus.WasResponsibleProvided,
                    preloadedMetadata?.Id,
                    PreloadedContentType.Sample,
                    preloadedSample?.FileName));
            }

            this.TempData[$"InterviewImportConfirmation-{preloadedMetadata.Id}"] = new PreloadedDataConfirmationModel
            {
                QuestionnaireId = model.QuestionnaireId,
                Version = model.QuestionnaireVersion,
                QuestionnaireTitle = questionnaireInfo?.Title,
                WasSupervsorProvided = verificationStatus.WasResponsibleProvided,
                Id = preloadedMetadata.Id,
                PreloadedContentType = PreloadedContentType.Sample,
                FileName = preloadedSample.FileName,
                EnumeratorsCount = verificationStatus.EnumeratorsCount,
                SupervisorsCount = verificationStatus.SupervisorsCount,
                InterviewsCount = verificationStatus.InterviewsCount
            };

            return this.RedirectToAction("InterviewImportConfirmation", new { id = preloadedMetadata.Id, questionnaireId = model.QuestionnaireId, version = model.QuestionnaireVersion });

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
                CurrentProcessId = this.interviewImportService.Status.InterviewImportProcessId
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

            if (!this.ModelState.IsValid)
            {
                var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);
                model.QuestionnaireTitle = questionnaireInfo.Title;
                return this.View(model);
            }

            var headquartersId = this.GlobalInfo.GetCurrentUser().Id;

            Task.Factory.StartNew(() =>
            {
                ThreadMarkerManager.MarkCurrentThreadAsIsolated();

                try
                {
                    this.interviewImportService.ImportInterviews(supervisorId: model.SupervisorId,
                        questionnaireIdentity: questionnaireIdentity, interviewImportProcessId: model.Id,
                        isPanel: model.PreloadedContentType == PreloadedContentType.Panel,
                        headquartersId: headquartersId);
                }
                finally
                {
                    ThreadMarkerManager.ReleaseCurrentThreadFromIsolation();
                }
            });

            return this.RedirectToAction("InterviewImportProgress", new { id = model.Id });
        }

        [ObserverNotAllowed]
        public ActionResult InterviewImportProgress(Guid id)
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;

            InterviewImportStatus status = this.interviewImportService.Status;

            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(new QuestionnaireIdentity(status.QuestionnaireId, status.QuestionnaireVersion));

            if (questionnaireInfo == null)
            {
                return RedirectToAction("Index");
            }

            return this.View(new PreloadedDataInterviewProgressModel
            {
                Status = status,
                QuestionnaireId = questionnaireInfo.QuestionnaireId,
                Version = questionnaireInfo.Version,
                QuestionnaireTitle = questionnaireInfo.Title
            });
        }
    }
}