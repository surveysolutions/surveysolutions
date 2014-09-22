﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire.BrowseItem;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.SampleImport;
using WB.Core.SharedKernels.SurveyManagement.Views.Survey;
using WB.Core.SharedKernels.SurveyManagement.Views.TakeNew;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Views.UsersAndQuestionnaires;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Headquarter")]
    public class HQController : BaseController
    {
        private readonly IViewFactory<AllUsersAndQuestionnairesInputModel, AllUsersAndQuestionnairesView> allUsersAndQuestionnairesFactory;
        private readonly IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireBrowseView> questionnaireBrowseViewFactory;
        private readonly IQueryableReadSideRepositoryWriter<InterviewSummary> interviews;
        private readonly ISampleImportService sampleImportService;
        private readonly IViewFactory<UserListViewInputModel, UserListView> supervisorsFactory;
        private readonly IViewFactory<TakeNewInterviewInputModel, TakeNewInterviewView> takeNewInterviewViewFactory;
        private readonly IPreloadingTemplateService preloadingTemplateService;
        private readonly IPreloadedDataRepository preloadedDataRepository;
        private readonly IPreloadedDataVerifier preloadedDataVerifier;
        private readonly IViewFactory<QuestionnaireItemInputModel, QuestionnaireBrowseItem> questionnaireBrowseItemFactory;

        public HQController(ICommandService commandService, IGlobalInfoProvider provider, ILogger logger,
            IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireBrowseView> questionnaireBrowseViewFactory,
            IViewFactory<TakeNewInterviewInputModel, TakeNewInterviewView> takeNewInterviewViewFactory,
            IViewFactory<UserListViewInputModel, UserListView> supervisorsFactory,
            ISampleImportService sampleImportService,
            IViewFactory<AllUsersAndQuestionnairesInputModel, AllUsersAndQuestionnairesView>
                allUsersAndQuestionnairesFactory,
            IPreloadingTemplateService preloadingTemplateService, IPreloadedDataRepository preloadedDataRepository,
            IPreloadedDataVerifier preloadedDataVerifier,
            IViewFactory<QuestionnaireItemInputModel, QuestionnaireBrowseItem> questionnaireBrowseItemFactory,
            IQueryableReadSideRepositoryWriter<InterviewSummary> interviews)
            : base(commandService, provider, logger)
        {
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.takeNewInterviewViewFactory = takeNewInterviewViewFactory;
            this.sampleImportService = sampleImportService;
            this.allUsersAndQuestionnairesFactory = allUsersAndQuestionnairesFactory;
            this.preloadingTemplateService = preloadingTemplateService;
            this.preloadedDataRepository = preloadedDataRepository;
            this.preloadedDataVerifier = preloadedDataVerifier;
            this.questionnaireBrowseItemFactory = questionnaireBrowseItemFactory;
            this.interviews = interviews;
            this.supervisorsFactory = supervisorsFactory;
        }

        public ActionResult Index()
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;

            var model = new HQDashboardModel
                {
                    Questionnaires = this.questionnaireBrowseViewFactory.Load(
                            new QuestionnaireBrowseInputModel() { PageSize = 1024 })
                };
            return this.View(model);
        }

        public ActionResult Delete(Guid id, long version)
        {
            var interviewByQuestionnaire =
              this.interviews.QueryAll(i => !i.IsDeleted && i.QuestionnaireId == id && i.QuestionnaireVersion == version);

            var interviewDeletionErrors = new List<Exception>();

            foreach (var interviewSummary in interviewByQuestionnaire)
            {
                try
                {
                    this.CommandService.Execute(new HardDeleteInterview(interviewSummary.InterviewId, this.GlobalInfo.GetCurrentUser().Id));
                }
                catch (Exception e)
                {
                    this.Logger.Error(e.Message, e);
                    interviewDeletionErrors.Add(e);
                }
            }

            if (interviewDeletionErrors.Any())
                throw new AggregateException(
                    string.Format("Failed to delete one or more interviews which were created from questionnaire {0} version {1}.", id.FormatGuid(), version),
                    interviewDeletionErrors);

            this.CommandService.Execute(new DeleteQuestionnaire(id, version));

            return this.RedirectToAction("Index");
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
            UserLight currentUser = this.GlobalInfo.GetCurrentUser();
            this.ViewBag.CurrentUser = new UsersViewItem { UserId = currentUser.Id, UserName = currentUser.Name };
            return this.View(this.Filters());
        }

        public ActionResult BatchUpload(Guid id, long version)
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;

            var viewModel = new BatchUploadModel()
            {
                QuestionnaireId = id,
                QuestionnaireVersion = version,
                FeaturedQuestions = this.questionnaireBrowseItemFactory.Load(new QuestionnaireItemInputModel(id, version)).FeaturedQuestions
            };

            return this.View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SampleBatchUpload(BatchUploadModel model)
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;

            if (!this.ModelState.IsValid)
            {
                return this.View("BatchUpload", model);
            }

            var preloadedDataId = this.preloadedDataRepository.Store(model.File.InputStream, model.File.FileName);
            var preloadedMetadata = this.preloadedDataRepository.GetPreloadedDataMetaInformationForSampleData(preloadedDataId);

            return this.View("ImportSample", new PreloadedMetaDataView(model.QuestionnaireId, model.QuestionnaireVersion, preloadedMetadata));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PanelBatchUpload(BatchUploadModel model)
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;

            if (!this.ModelState.IsValid)
            {
                return this.View("BatchUpload", model);
            }

            var preloadedDataId = this.preloadedDataRepository.Store(model.File.InputStream, model.File.FileName);
            var preloadedMetadata = this.preloadedDataRepository.GetPreloadedDataMetaInformationForPanelData(preloadedDataId);

            return this.View("ImportSample", new PreloadedMetaDataView(model.QuestionnaireId, model.QuestionnaireVersion, preloadedMetadata));
        }

        public ActionResult TemplateDownload(Guid id, long version)
        {
            var pathToFile = this.preloadingTemplateService.GetFilePathToPreloadingTemplate(id, version);
            return this.File(pathToFile, "application/zip", fileDownloadName: Path.GetFileName(pathToFile));
        }

        public ActionResult VerifySample(Guid questionnaireId, long version, string id)
        {
            var errors = this.preloadedDataVerifier.VerifySample(questionnaireId, version, this.preloadedDataRepository.GetPreloadedDataOfSample(id));
            this.ViewBag.SupervisorList =
              this.supervisorsFactory.Load(new UserListViewInputModel { Role = UserRoles.Supervisor, PageSize = int.MaxValue }).Items;
            return this.View(new PreloadedDataVerificationErrorsView(questionnaireId, version, errors.ToArray(), id, PreloadedContentType.Sample));
        }

        public ActionResult VerifyPanel(Guid questionnaireId, long version, string id)
        {
            var errors = this.preloadedDataVerifier.VerifyPanel(questionnaireId, version, this.preloadedDataRepository.GetPreloadedDataOfPanel(id));
            this.ViewBag.SupervisorList =
              this.supervisorsFactory.Load(new UserListViewInputModel { Role = UserRoles.Supervisor, PageSize = int.MaxValue }).Items;
            return this.View("VerifySample", new PreloadedDataVerificationErrorsView(questionnaireId, version, errors.ToArray(), id, PreloadedContentType.Panel));
        }

        public ActionResult ImportPanelData(Guid questionnaireId, long version, string id, Guid responsibleSupervisor)
        {
            this.sampleImportService.CreatePanel(questionnaireId, version, id, this.preloadedDataRepository.GetPreloadedDataOfPanel(id),
                this.GlobalInfo.GetCurrentUser().Id, responsibleSupervisor);
            return this.RedirectToAction("SampleCreationResult", new { id });
        }

        public ActionResult ImportSampleData(Guid questionnaireId, long version, string id, Guid responsibleSupervisor)
        {
            this.sampleImportService.CreateSample(questionnaireId, version, id, this.preloadedDataRepository.GetPreloadedDataOfSample(id),
                this.GlobalInfo.GetCurrentUser().Id, responsibleSupervisor);
            return this.RedirectToAction("SampleCreationResult", new { id });
        }

        public ActionResult SampleCreationResult(string id)
        {
            SampleCreationStatus result = this.sampleImportService.GetSampleCreationStatus(id);
            return this.View(result);
        }

        public JsonResult GetSampleCreationStatus(string id)
        {
            return this.Json(this.sampleImportService.GetSampleCreationStatus(id));
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

            AllUsersAndQuestionnairesView usersAndQuestionnaires =
                this.allUsersAndQuestionnairesFactory.Load(new AllUsersAndQuestionnairesInputModel());

            return this.View(usersAndQuestionnaires.Users);
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
                    Users =
                        this.supervisorsFactory.Load(new UserListViewInputModel { PageSize = int.MaxValue })
                            .Items.Where(u => !u.IsLockedBySupervisor && !u.IsLockedByHQ)
                            .Select(u => new UsersViewItem
                                {
                                    UserId = u.UserId,
                                    UserName = u.UserName
                                }),
                    Responsibles = usersAndQuestionnaires.Users,
                    Templates = usersAndQuestionnaires.Questionnaires,
                    Statuses = statuses
                };
        }
    }
}