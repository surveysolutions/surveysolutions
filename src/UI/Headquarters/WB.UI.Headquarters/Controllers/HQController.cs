﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Commands;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.SampleImport;
using WB.Core.SharedKernels.SurveyManagement.Views.Survey;
using WB.Core.SharedKernels.SurveyManagement.Views.TakeNew;
using WB.Core.SharedKernels.SurveyManagement.Views.UsersAndQuestionnaires;
using WB.Core.SharedKernels.SurveyManagement.Web.Code.Security;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Headquarters.Services;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [LimitsFilter]
    [Authorize(Roles = "Administrator, Headquarter")]
    public class HQController : BaseController
    {
        private readonly IViewFactory<AllUsersAndQuestionnairesInputModel, AllUsersAndQuestionnairesView> allUsersAndQuestionnairesFactory;
        private readonly Func<ISampleImportService> sampleImportServiceFactory;
        private readonly IViewFactory<TakeNewInterviewInputModel, TakeNewInterviewView> takeNewInterviewViewFactory;
        private readonly IPreloadingTemplateService preloadingTemplateService;
        private readonly IPreloadedDataRepository preloadedDataRepository;
        private readonly IPreloadedDataVerifier preloadedDataVerifier;
        private readonly IViewFactory<SampleUploadViewInputModel, SampleUploadView> sampleUploadViewFactory;
        private readonly InterviewDataExportSettings interviewDataExportSettings;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IInterviewImportService interviewImportService;

        public HQController(ICommandService commandService, IGlobalInfoProvider provider, ILogger logger,
            IViewFactory<TakeNewInterviewInputModel, TakeNewInterviewView> takeNewInterviewViewFactory,
            Func<ISampleImportService> sampleImportServiceFactory,
            IViewFactory<AllUsersAndQuestionnairesInputModel, AllUsersAndQuestionnairesView> allUsersAndQuestionnairesFactory,
            IPreloadingTemplateService preloadingTemplateService,
            IPreloadedDataRepository preloadedDataRepository,
            IPreloadedDataVerifier preloadedDataVerifier,
            IViewFactory<SampleUploadViewInputModel, SampleUploadView> sampleUploadViewFactory,
            InterviewDataExportSettings interviewDataExportSettings,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IInterviewImportService interviewImportService)
            : base(commandService, provider, logger)
        {
            this.takeNewInterviewViewFactory = takeNewInterviewViewFactory;
            this.allUsersAndQuestionnairesFactory = allUsersAndQuestionnairesFactory;
            this.preloadingTemplateService = preloadingTemplateService;
            this.preloadedDataRepository = preloadedDataRepository;
            this.preloadedDataVerifier = preloadedDataVerifier;
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.interviewImportService = interviewImportService;
            this.sampleUploadViewFactory = sampleUploadViewFactory;
            this.sampleImportServiceFactory = sampleImportServiceFactory;
        }

        public ActionResult Index()
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;
            this.ViewBag.EnableInterviewHistory = this.interviewDataExportSettings.EnableInterviewHistory;
            return this.View();
        }

        public ActionResult Interviews(Guid? questionnaireId)
        {
            if (questionnaireId.HasValue)
            {
                this.Success(
                    string.Format(
                        @"Interview was successfully created. <a class=""btn btn-success"" href=""{0}""><i class=""icon-plus""></i> Create one more?</a>",
                        this.Url.Action("TakeNew", "HQ", new { id = questionnaireId.Value })));
            }
            this.ViewBag.ActivePage = MenuItem.Docs;
            return this.View(this.Filters());
        }

        public ActionResult BatchUpload(Guid id, long version)
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;

            var featuredQuestionItems = this.sampleUploadViewFactory.Load(new SampleUploadViewInputModel(id, version)).ColumnListToPreload;
            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(new QuestionnaireIdentity(id, version));

            var viewModel = new BatchUploadModel()
            {
                QuestionnaireId = id,
                QuestionnaireVersion = version,
                QuestionnaireTitle = questionnaireInfo?.Title,
                FeaturedQuestions = featuredQuestionItems
            };

            return this.View(viewModel);
        }

        public ActionResult CloneQuestionnaire(Guid id, long version)
        {
            QuestionnaireBrowseItem questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(new QuestionnaireIdentity(id, version));

            if (questionnaireBrowseItem == null)
                return new HttpNotFoundResult($"Questionnaire with id {id.FormatGuid()} and version {version} is not found.");

            return this.View(new CloneQuestionnaireModel(id, version, questionnaireBrowseItem.Title, questionnaireBrowseItem.AllowCensusMode));
        }

        [HttpPost]
        [PreventDoubleSubmit]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public ActionResult CloneQuestionnaire(CloneQuestionnaireModel model)
        {
            if (!this.ModelState.IsValid)
                return this.View(model);

            try
            {
                this.CommandService.Execute(new CloneQuestionnaire(
                    model.Id, model.Version, model.NewTitle, userId: this.GlobalInfo.GetCurrentUser().Id));
            }
            catch (QuestionnaireException exception)
            {
                this.Error(exception.Message);
                return this.View(model);
            }
            catch (Exception exception)
            {
                this.Logger.Error($"Unexpected error occurred while cloning questionnaire (id: {model.Id}, version: {model.Version}).", exception);
                this.Error(global::Resources.CloneQuestionnaire.UnexpectedError);
                return this.View(model);
            }

            this.Success(
                model.NewTitle == model.OriginalTitle
                    ? $"Questionnaire '{model.OriginalTitle}' was successfully cloned."
                    : $"Questionnaire '{model.OriginalTitle}' was successfully cloned with new title '{model.NewTitle}'.");

            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public ActionResult PanelBatchUpload(BatchUploadModel model)
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;

            if (!this.ModelState.IsValid)
            {
                return this.RedirectToAction("BatchUpload",
                    new {id = model.QuestionnaireId, version = model.QuestionnaireVersion});
            }

            var preloadedDataId = this.preloadedDataRepository.Store(model.File.InputStream, model.File.FileName);
            var preloadedMetadata = this.preloadedDataRepository.GetPreloadedDataMetaInformationForPanelData(preloadedDataId);

            //clean up for security reasons
            if (preloadedMetadata == null)
            {
                this.preloadedDataRepository.DeletePreloadedDataOfSample(preloadedDataId);
            }

            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(new QuestionnaireIdentity(model.QuestionnaireId, model.QuestionnaireVersion));

            return this.View("ImportSample", new PreloadedMetaDataView(model.QuestionnaireId, model.QuestionnaireVersion, questionnaireInfo?.Title, preloadedMetadata));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public ActionResult SampleBatchUpload(BatchUploadModel model)
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;

            if (!this.ModelState.IsValid)
            {
                return this.RedirectToAction("BatchUpload",
                    new { id = model.QuestionnaireId, version = model.QuestionnaireVersion });
            }

            var preloadedDataId = this.preloadedDataRepository.Store(model.File.InputStream, model.File.FileName);
            var preloadedMetadata = this.preloadedDataRepository.GetPreloadedDataMetaInformationForSampleData(preloadedDataId);

            //clean up for security reasons
            if (preloadedMetadata == null)
            {
                this.preloadedDataRepository.DeletePreloadedDataOfSample(preloadedDataId);
            }

            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(new QuestionnaireIdentity(model.QuestionnaireId, model.QuestionnaireVersion));

            return this.View("ImportSample", new PreloadedMetaDataView(model.QuestionnaireId, model.QuestionnaireVersion, questionnaireInfo?.Title, preloadedMetadata));
        }

        public ActionResult TemplateDownload(Guid id, long version)
        {
            var pathToFile = this.preloadingTemplateService.GetFilePathToPreloadingTemplate(id, version);
            return this.File(pathToFile, "application/zip", fileDownloadName: Path.GetFileName(pathToFile));
        }

        public ActionResult VerifySample(Guid questionnaireId, long version, string id)
        {
            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(new QuestionnaireIdentity(questionnaireId, version));

            if (this.interviewImportService.Status.InterviewImportProcessId == id)
            {
                var inProgressModel = new PreloadedDataVerificationErrorsView(questionnaireId, version,
                    questionnaireInfo?.Title, new PreloadedDataVerificationError[0],
                    true, id, PreloadedContentType.Sample);

                return this.View(inProgressModel);
            }

            var preloadedSample = this.preloadedDataRepository.GetPreloadedDataOfSample(id);
            //null is handled inside 
            var verificationStatus = this.preloadedDataVerifier.VerifySample(questionnaireId, version, preloadedSample);

            //clean up for security reasons
            if (verificationStatus.Errors.Any())
            {
                this.preloadedDataRepository.DeletePreloadedDataOfSample(id);
            }

            var model = new PreloadedDataVerificationErrorsView(questionnaireId, version, questionnaireInfo?.Title, verificationStatus.Errors.ToArray(), 
                verificationStatus.WasResponsibleProvided, id, PreloadedContentType.Sample);
            return this.View(model);
        }

        public ActionResult VerifyPanel(Guid questionnaireId, long version, string id)
        {
            var preloadedPanelData = this.preloadedDataRepository.GetPreloadedDataOfPanel(id);
            var verificationStatus = this.preloadedDataVerifier.VerifyPanel(questionnaireId, version, preloadedPanelData);
            
            //clean up for security reasons
            if (verificationStatus.Errors.Any())
            {
                this.preloadedDataRepository.DeletePreloadedDataOfPanel(id);
            }

            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(new QuestionnaireIdentity(questionnaireId, version));

            var model = new PreloadedDataVerificationErrorsView(questionnaireId, version, questionnaireInfo?.Title, verificationStatus.Errors.ToArray(), 
                verificationStatus.WasResponsibleProvided, id, PreloadedContentType.Panel);

            return this.View(model);
        }

        public ActionResult SampleCreationResult(string id)
        {
            var sampleImportService = this.sampleImportServiceFactory.Invoke();

            SampleCreationStatus result = sampleImportService.GetSampleCreationStatus(id);

            return this.View(result);
        }

        public JsonResult GetSampleCreationStatus(string id)
        {
            var sampleImportService = this.sampleImportServiceFactory.Invoke();

            return this.Json(sampleImportService.GetSampleCreationStatus(id));
        }

        public ActionResult TakeNew(Guid id, long? version)
        {
            Guid key = id;
            UserLight user = this.GlobalInfo.GetCurrentUser();
            TakeNewInterviewView model = this.takeNewInterviewViewFactory.Load(new TakeNewInterviewInputModel(key, version, user.Id));
            return this.View(model);
        }

        public ActionResult SurveysAndStatuses()
        {
            this.ViewBag.ActivePage = MenuItem.Surveys;

            return this.View();
        }

        public ActionResult SupervisorsAndStatuses()
        {
            this.ViewBag.ActivePage = MenuItem.Summary;

            AllUsersAndQuestionnairesView usersAndQuestionnaires =
                this.allUsersAndQuestionnairesFactory.Load(new AllUsersAndQuestionnairesInputModel());

            return this.View(usersAndQuestionnaires.Questionnaires);
        }

        public ActionResult MapReport()
        {
            this.ViewBag.ActivePage = MenuItem.MapReport;

            return this.View();
        }

        public ActionResult InterviewsChart()
        {
            this.ViewBag.ActivePage = MenuItem.InterviewsChart;

            return this.View(this.Filters());
        }

        public ActionResult Status()
        {
            this.ViewBag.ActivePage = MenuItem.Statuses;
            return this.View(StatusHelper.GetOnlyActualSurveyStatusViewItems());
        }

        private DocumentFilter Filters()
        {
            IEnumerable<SurveyStatusViewItem> statuses = StatusHelper.GetOnlyActualSurveyStatusViewItems();

            AllUsersAndQuestionnairesView usersAndQuestionnaires =
                this.allUsersAndQuestionnairesFactory.Load(new AllUsersAndQuestionnairesInputModel());

            return new DocumentFilter
            {
                Templates = usersAndQuestionnaires.Questionnaires,
                Statuses = statuses
            };
        }

    }
}