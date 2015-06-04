using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;

using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.Storage;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.SampleImport;
using WB.Core.SharedKernels.SurveyManagement.Views.Survey;
using WB.Core.SharedKernels.SurveyManagement.Views.TakeNew;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Views.UsersAndQuestionnaires;
using WB.Core.SharedKernels.SurveyManagement.Web.Code.Security;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter")]
    public class HQController : BaseController
    {
        private readonly IViewFactory<AllUsersAndQuestionnairesInputModel, AllUsersAndQuestionnairesView> allUsersAndQuestionnairesFactory;
        private readonly Func<ISampleImportService> sampleImportServiceFactory;
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
            Func<ISampleImportService> sampleImportServiceFactory,
            IViewFactory<AllUsersAndQuestionnairesInputModel, AllUsersAndQuestionnairesView> allUsersAndQuestionnairesFactory,
            IPreloadingTemplateService preloadingTemplateService,
            IPreloadedDataRepository preloadedDataRepository,
            IPreloadedDataVerifier preloadedDataVerifier,
            IViewFactory<QuestionnaireItemInputModel, QuestionnaireBrowseItem> questionnaireBrowseItemFactory,
            InterviewHistorySettings interviewHistorySettings)
            : base(commandService, provider, logger)
        {
            this.takeNewInterviewViewFactory = takeNewInterviewViewFactory;
            this.allUsersAndQuestionnairesFactory = allUsersAndQuestionnairesFactory;
            this.preloadingTemplateService = preloadingTemplateService;
            this.preloadedDataRepository = preloadedDataRepository;
            this.preloadedDataVerifier = preloadedDataVerifier;
            this.questionnaireBrowseItemFactory = questionnaireBrowseItemFactory;
            this.interviewHistorySettings = interviewHistorySettings;
            this.sampleImportServiceFactory = sampleImportServiceFactory;
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

            var featuredQuestionItems = this.questionnaireBrowseItemFactory.Load(new QuestionnaireItemInputModel(id, version)).FeaturedQuestions;
            var viewModel = new BatchUploadModel()
            {
                QuestionnaireId = id,
                QuestionnaireVersion = version,
                FeaturedQuestions = new List<FeaturedQuestionItem>(featuredQuestionItems)
            };

            return this.View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
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
        [ObserverNotAllowed]
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
            var verificationStatus = this.preloadedDataVerifier.VerifySample(questionnaireId, version, preloadedSample);

            this.ViewBag.SupervisorList = this.supervisorsFactory.Load(new UserListViewInputModel { Role = UserRoles.Supervisor, PageSize = int.MaxValue, Order = "UserName"}).Items;

            //clean up for security reasons
            if (verificationStatus.Errors.Any())
            {
                this.preloadedDataRepository.DeletePreloadedDataOfSample(id);
            }

            var model = new PreloadedDataVerificationErrorsView(questionnaireId, version, verificationStatus.Errors.ToArray(), 
                verificationStatus.WasSupervisorProvided, id, PreloadedContentType.Sample);
            return this.View(model);
        }

        public ActionResult VerifyPanel(Guid questionnaireId, long version, string id)
        {
            var preloadedPanelData = this.preloadedDataRepository.GetPreloadedDataOfPanel(id);
            var verificationStatus = this.preloadedDataVerifier.VerifyPanel(questionnaireId, version, preloadedPanelData);
            this.ViewBag.SupervisorList =
              this.supervisorsFactory.Load(new UserListViewInputModel { Role = UserRoles.Supervisor, PageSize = int.MaxValue, Order = "UserName" }).Items;

            //clean up for security reasons
            if (verificationStatus.Errors.Any())
            {
                this.preloadedDataRepository.DeletePreloadedDataOfPanel(id);
            }

            var model = new PreloadedDataVerificationErrorsView(questionnaireId, version, verificationStatus.Errors.ToArray(), 
                verificationStatus.WasSupervisorProvided, id, PreloadedContentType.Panel);

            return this.View("VerifySample", model);
        }

        [ObserverNotAllowed]
        public ActionResult ImportPanelData(Guid questionnaireId, long version, string id, Guid? responsibleSupervisor)
        {
            PreloadedDataByFile[] preloadedData = this.preloadedDataRepository.GetPreloadedDataOfPanel(id);
            Guid responsibleHeadquarterId = this.GlobalInfo.GetCurrentUser().Id;

            new Task(() =>
            {
                IsolatedThreadManager.MarkCurrentThreadAsIsolated();

                try
                {
                    var sampleImportService = this.sampleImportServiceFactory.Invoke();

                    sampleImportService.CreatePanel(
                        questionnaireId,
                        version,
                        id,
                        preloadedData,
                        responsibleHeadquarterId,
                        responsibleSupervisor);
                }
                finally
                {
                    IsolatedThreadManager.ReleaseCurrentThreadFromIsolation();
                }
            }).Start();

            return this.RedirectToAction("SampleCreationResult", new { id });
        }

        [ObserverNotAllowed]
        public ActionResult ImportSampleData(Guid questionnaireId, long version, string id, Guid? responsibleSupervisor)
        {
            PreloadedDataByFile preloadedData = this.preloadedDataRepository.GetPreloadedDataOfSample(id);
            Guid responsibleHeadquarterId = this.GlobalInfo.GetCurrentUser().Id;

            new Task(() =>
            {
                IsolatedThreadManager.MarkCurrentThreadAsIsolated();

                try
                {
                    var sampleImportService = this.sampleImportServiceFactory.Invoke();

                    sampleImportService.CreateSample(
                        questionnaireId,
                        version,
                        id,
                        preloadedData,
                        responsibleHeadquarterId,
                        responsibleSupervisor);
                }
                finally
                {
                    IsolatedThreadManager.ReleaseCurrentThreadFromIsolation();
                }
            }).Start();
            
            return this.RedirectToAction("SampleCreationResult", new { id });
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

        public ActionResult DataExport()
        {
            this.ViewBag.ActivePage = MenuItem.DataExport;
            this.ViewBag.EnableInterviewHistory = interviewHistorySettings.EnableInterviewHistory;

            AllUsersAndQuestionnairesView usersAndQuestionnaires =
                this.allUsersAndQuestionnairesFactory.Load(new AllUsersAndQuestionnairesInputModel());

            return this.View(usersAndQuestionnaires.Questionnaires);
        }
    }
}