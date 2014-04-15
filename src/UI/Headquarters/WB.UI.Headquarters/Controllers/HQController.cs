﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire.BrowseItem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.SampleRecordsAccessors;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.SampleImport;
using WB.Core.SharedKernels.SurveyManagement.Views.Survey;
using WB.Core.SharedKernels.SurveyManagement.Views.TakeNew;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Views.UsersAndQuestionnaires;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Headquarter")]
    public class HQController : BaseController
    {
        private readonly IViewFactory<AllUsersAndQuestionnairesInputModel, AllUsersAndQuestionnairesView> allUsersAndQuestionnairesFactory;
        private readonly IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireBrowseView> questionnaireBrowseViewFactory;
        private readonly IViewFactory<QuestionnairePreloadingDataInputModel, QuestionnairePreloadingDataItem> questionnairePreloadingDataItemFactory;
      
        private readonly ISampleImportService sampleImportService;
        private readonly IViewFactory<UserListViewInputModel, UserListView> supervisorsFactory;
        private readonly IViewFactory<TakeNewInterviewInputModel, TakeNewInterviewView> takeNewInterviewViewFactory;
        private readonly IPreloadingTemplateService preloadingTemplateService;
        private readonly IPreloadedDataRepository preloadedDataRepository;
        private readonly IPreloadedDataVerifier preloadedDataVerifier;


        public HQController(ICommandService commandService, IGlobalInfoProvider provider, ILogger logger,
            IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireBrowseView> questionnaireBrowseViewFactory,
            IViewFactory<TakeNewInterviewInputModel, TakeNewInterviewView> takeNewInterviewViewFactory,
            IViewFactory<UserListViewInputModel, UserListView> supervisorsFactory,
            ISampleImportService sampleImportService,
            IViewFactory<AllUsersAndQuestionnairesInputModel, AllUsersAndQuestionnairesView>
                allUsersAndQuestionnairesFactory,
            IViewFactory<QuestionnairePreloadingDataInputModel, QuestionnairePreloadingDataItem> questionnairePreloadingDataItemFactory,
            IPreloadingTemplateService preloadingTemplateService, IPreloadedDataRepository preloadedDataRepository,
            IPreloadedDataVerifier preloadedDataVerifier)
            : base(commandService, provider, logger)
        {
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.takeNewInterviewViewFactory = takeNewInterviewViewFactory;
            this.sampleImportService = sampleImportService;
            this.allUsersAndQuestionnairesFactory = allUsersAndQuestionnairesFactory;
            this.questionnairePreloadingDataItemFactory = questionnairePreloadingDataItemFactory;
            this.preloadingTemplateService = preloadingTemplateService;
            this.preloadedDataRepository = preloadedDataRepository;
            this.preloadedDataVerifier = preloadedDataVerifier;
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
                QuestionnaireVersion = version
            };

            return this.View(viewModel);
        }

        [HttpPost]
        public ActionResult BatchUpload(BatchUploadModel model)
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var preloadedDataId = preloadedDataRepository.Store(model.File.InputStream, model.File.FileName);
            var preloadedMetadata = preloadedDataRepository.GetPreloadedDataMetaInformation(preloadedDataId);

            return this.View("ImportSample", new PreloadedMetaDataView(model.QuestionnaireId, model.QuestionnaireVersion, preloadedMetadata));
        }

        public ActionResult TemplateDownload(Guid id, long version)
        {
            var pathToFile = preloadingTemplateService.GetFilePathToPreloadingTemplate(id, version);
            return this.File(pathToFile, "application/zip", fileDownloadName: Path.GetFileName(pathToFile));
        }

        public ActionResult VerifySample(Guid questionnaireId, long version, string id)
        {
            var errors = preloadedDataVerifier.Verify(questionnaireId, version, preloadedDataRepository.GetPreloadedData(id));
            this.ViewBag.SupervisorList =
              this.supervisorsFactory.Load(new UserListViewInputModel { Role = UserRoles.Supervisor, PageSize = int.MaxValue }).Items;
            return this.View(new PreloadedDataVerificationErrorsView(questionnaireId, version, errors.ToArray(), id));
        }


        public ActionResult ImportPreloadedData(Guid questionnaireId, long version, string id, Guid responsibleSupervisor)
        {
            this.sampleImportService.CreateSample(questionnaireId, version, id, preloadedDataRepository.GetPreloadedData(id),
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


        public ActionResult TakeNew(Guid id)
        {
            Guid key = id;
            UserLight user = this.GlobalInfo.GetCurrentUser();
            TakeNewInterviewView model = this.takeNewInterviewViewFactory.Load(new TakeNewInterviewInputModel(key, user.Id));
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
                            .Items.Where(u => !u.IsLocked)
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