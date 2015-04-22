using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.SampleImport;
using WB.Core.SharedKernels.SurveyManagement.Views.Survey;
using WB.Core.SharedKernels.SurveyManagement.Views.TakeNew;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Views.UsersAndQuestionnaires;
using WB.Core.SharedKernels.SurveyManagement.Web.Code.Security;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter")]
    public class HQController : BaseController
    {
        private readonly IViewFactory<AllUsersAndQuestionnairesInputModel, AllUsersAndQuestionnairesView> allUsersAndQuestionnairesFactory;
        private readonly ISampleImportService sampleImportService;
        private readonly IUserListViewFactory supervisorsFactory;
        private readonly IViewFactory<TakeNewInterviewInputModel, TakeNewInterviewView> takeNewInterviewViewFactory;
        private readonly IPreloadingTemplateService preloadingTemplateService;
        private readonly IPreloadedDataRepository preloadedDataRepository;
        private readonly IPreloadedDataVerifier preloadedDataVerifier;
        private readonly IViewFactory<QuestionnaireItemInputModel, QuestionnaireBrowseItem> questionnaireBrowseItemFactory;
        private readonly InterviewHistorySettings interviewHistorySettings;

        public HQController(ICommandService commandService, IGlobalInfoProvider provider, ILogger logger,
            IViewFactory<TakeNewInterviewInputModel, TakeNewInterviewView> takeNewInterviewViewFactory,
            IUserListViewFactory supervisorsFactory,
            ISampleImportService sampleImportService,
            IViewFactory<AllUsersAndQuestionnairesInputModel, AllUsersAndQuestionnairesView>
                allUsersAndQuestionnairesFactory,
            IPreloadingTemplateService preloadingTemplateService, IPreloadedDataRepository preloadedDataRepository,
            IPreloadedDataVerifier preloadedDataVerifier,
            IViewFactory<QuestionnaireItemInputModel, QuestionnaireBrowseItem> questionnaireBrowseItemFactory, InterviewHistorySettings interviewHistorySettings)
            : base(commandService, provider, logger)
        {
            this.takeNewInterviewViewFactory = takeNewInterviewViewFactory;
            this.sampleImportService = sampleImportService;
            this.allUsersAndQuestionnairesFactory = allUsersAndQuestionnairesFactory;
            this.preloadingTemplateService = preloadingTemplateService;
            this.preloadedDataRepository = preloadedDataRepository;
            this.preloadedDataVerifier = preloadedDataVerifier;
            this.questionnaireBrowseItemFactory = questionnaireBrowseItemFactory;
            this.interviewHistorySettings = interviewHistorySettings;
            this.supervisorsFactory = supervisorsFactory;
        }

        public ActionResult Index()
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;
            this.ViewBag.EnableInterviewHistory = interviewHistorySettings.EnableInterviewHistory;
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

            if (User.Identity.IsObserver())
            {
                this.Error("You cannot perform any operation in observer mode.");
                return this.View("BatchUpload", model);
            }

            var preloadedDataId = this.preloadedDataRepository.Store(model.File.InputStream, model.File.FileName);
            var preloadedMetadata = this.preloadedDataRepository.GetPreloadedDataMetaInformationForSampleData(preloadedDataId);

            //clean up for security reasons
            if (preloadedMetadata == null)
            {
                this.preloadedDataRepository.DeletePreloadedDataOfSample(preloadedDataId);
            }

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

            if (User.Identity.IsObserver())
            {
                this.Error("You cannot perform any operation in observer mode.");
                return this.View("BatchUpload", model);
            }

            var preloadedDataId = this.preloadedDataRepository.Store(model.File.InputStream, model.File.FileName);
            var preloadedMetadata = this.preloadedDataRepository.GetPreloadedDataMetaInformationForPanelData(preloadedDataId);

            //clean up for security reasons
            if (preloadedMetadata == null)
            {
                this.preloadedDataRepository.DeletePreloadedDataOfSample(preloadedDataId);
            }

            return this.View("ImportSample", new PreloadedMetaDataView(model.QuestionnaireId, model.QuestionnaireVersion, preloadedMetadata));
        }

        public ActionResult TemplateDownload(Guid id, long version)
        {
            var pathToFile = this.preloadingTemplateService.GetFilePathToPreloadingTemplate(id, version);
            return this.File(pathToFile, "application/zip", fileDownloadName: Path.GetFileName(pathToFile));
        }

        public ActionResult VerifySample(Guid questionnaireId, long version, string id)
        {
            var preloadedSample = this.preloadedDataRepository.GetPreloadedDataOfSample(id);
            //null is handled inside 
            var errors = this.preloadedDataVerifier.VerifySample(questionnaireId, version, preloadedSample).ToList();

            this.ViewBag.SupervisorList = this.supervisorsFactory.Load(new UserListViewInputModel { Role = UserRoles.Supervisor, PageSize = int.MaxValue, Order = "UserName"}).Items;

            //clean up for security reasons
            if (errors.Any())
            {
                this.preloadedDataRepository.DeletePreloadedDataOfSample(id);
            }

            return this.View(new PreloadedDataVerificationErrorsView(questionnaireId, version, errors.ToArray(), id, PreloadedContentType.Sample));
        }

        public ActionResult VerifyPanel(Guid questionnaireId, long version, string id)
        {
            var preloadedPanelData = this.preloadedDataRepository.GetPreloadedDataOfPanel(id);
            var errors = this.preloadedDataVerifier.VerifyPanel(questionnaireId, version, preloadedPanelData).ToList();
            this.ViewBag.SupervisorList =
              this.supervisorsFactory.Load(new UserListViewInputModel { Role = UserRoles.Supervisor, PageSize = int.MaxValue, Order = "UserName" }).Items;

            //clean up for security reasons
            if (errors.Any())
            {
                this.preloadedDataRepository.DeletePreloadedDataOfPanel(id);
            }
            
            return this.View("VerifySample", new PreloadedDataVerificationErrorsView(questionnaireId, version, errors.ToArray(), id, PreloadedContentType.Panel));
        }

        public ActionResult ImportPanelData(Guid questionnaireId, long version, string id, Guid responsibleSupervisor)
        {
            if (User.Identity.IsObserver())
            {
                this.Error("You cannot perform any operation in observer mode.");
                return this.View("VerifySample", new PreloadedDataVerificationErrorsView(questionnaireId, version, null, id, PreloadedContentType.Panel));
            }

            this.sampleImportService.CreatePanel(questionnaireId, version, id, this.preloadedDataRepository.GetPreloadedDataOfPanel(id),
                this.GlobalInfo.GetCurrentUser().Id, responsibleSupervisor);

            return this.RedirectToAction("SampleCreationResult", new { id });
        }

        public ActionResult ImportSampleData(Guid questionnaireId, long version, string id, Guid responsibleSupervisor)
        {
            if (User.Identity.IsObserver())
            {
                this.Error("You cannot perform any operation in observer mode.");
                return this.View("VerifySample", new PreloadedDataVerificationErrorsView(questionnaireId, version, null, id, PreloadedContentType.Panel));
            }

            this.sampleImportService.CreateSample(questionnaireId,
                    version,
                    id,
                    this.preloadedDataRepository.GetPreloadedDataOfSample(id),
                    this.GlobalInfo.GetCurrentUser().Id,
                    responsibleSupervisor);
            
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