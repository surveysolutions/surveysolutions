using System.Collections.Generic;
using System.Web;
using Core.Supervisor.Views;
using Core.Supervisor.Views.Summary;
using Core.Supervisor.Views.Survey;
using Core.Supervisor.Views.TakeNew;
using Core.Supervisor.Views.User;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire.BrowseItem;

namespace Web.Supervisor.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Mvc;

    using Core.Supervisor.Views.Assign;

    using Main.Core.Commands.Questionnaire.Completed;
    using Main.Core.Entities.SubEntities;
    using Main.Core.View;
    using Ncqrs.Commanding.ServiceModel;

    using Questionnaire.Core.Web.Helpers;
    using Web.Supervisor.Models;

    [Authorize(Roles = "Headquarter")]
    public class HQController : BaseController
    {
        private readonly IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireBrowseView> questionnaireBrowseViewFactory;
        private readonly IViewFactory<UserListViewInputModel, UserListView> userListViewFactory;
        private readonly IViewFactory<AssignSurveyInputModel, AssignSurveyView> assignSurveyViewFactory;
        private readonly IViewFactory<TakeNewInterviewInputModel, TakeNewInterviewView> takeNewInterviewViewFactory;
        private readonly IViewFactory<SurveyUsersViewInputModel, SurveyUsersView> surveyUsersViewFactory;
        private readonly IViewFactory<SummaryTemplatesInputModel, SummaryTemplatesView> summaryTemplatesViewFactory;
        private readonly IViewFactory<UserListViewInputModel, UserListView> supervisorsFactory;

        public HQController(ICommandService commandService, IGlobalInfoProvider provider, ILogger logger,
            IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireBrowseView> questionnaireBrowseViewFactory,
            IViewFactory<UserListViewInputModel, UserListView> userListViewFactory,
            IViewFactory<AssignSurveyInputModel, AssignSurveyView> assignSurveyViewFactory,
            IViewFactory<SurveyUsersViewInputModel, SurveyUsersView> surveyUsersViewFactory,
            IViewFactory<SummaryTemplatesInputModel, SummaryTemplatesView> summaryTemplatesViewFactory,
            IViewFactory<TakeNewInterviewInputModel, TakeNewInterviewView> takeNewInterviewViewFactory, IViewFactory<UserListViewInputModel, UserListView> supervisorsFactory)
            : base(commandService, provider, logger)
        {
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.userListViewFactory = userListViewFactory;
            this.assignSurveyViewFactory = assignSurveyViewFactory;
            this.surveyUsersViewFactory = surveyUsersViewFactory;
            this.summaryTemplatesViewFactory = summaryTemplatesViewFactory;
            this.takeNewInterviewViewFactory = takeNewInterviewViewFactory;
            this.supervisorsFactory = supervisorsFactory;
        }

        public ActionResult Index()
        {
            var model = new HQDashboardModel
                {
                    Questionnaires =
                        this.questionnaireBrowseViewFactory.Load(
                            new QuestionnaireBrowseInputModel()),
                    Teams = this.userListViewFactory.Load(new UserListViewInputModel { Role = UserRoles.Supervisor })
                };
            return this.View(model);

        }

        public ActionResult Interviews(Guid? templateId)
        {
            if (templateId.HasValue)
            {
                this.Success(string.Format(@"Interview was successfully created. <a class=""btn btn-success"" href=""{0}""><i class=""icon-plus""></i> Create one more?</a>", Url.Action("TakeNew", "HQ", new { id = templateId.Value })));
            }
            ViewBag.ActivePage = MenuItem.Docs;
            return this.View(Filters());
        }

        public ActionResult TakeNew(Guid id)
        {
            Guid key = id;
            var user = this.GlobalInfo.GetCurrentUser();
            var model = this.takeNewInterviewViewFactory.Load(new TakeNewInterviewInputModel(key, user.Id));
            model.CurrentUser = user;
            return this.View(model);
        }

        public ActionResult Surveys()
        {
            ViewBag.ActivePage = MenuItem.Surveys;
            return
                this.View(
                    this.surveyUsersViewFactory.Load(new SurveyUsersViewInputModel(this.GlobalInfo.GetCurrentUser().Id,
                        ViewerStatus.Headquarter)).Items);
        }


        public ActionResult Assign(Guid id)
        {
            var user = this.GlobalInfo.GetCurrentUser();
            var model = this.assignSurveyViewFactory.Load(new AssignSurveyInputModel(id, user.Id));
            return this.View(model);
        }

        [HttpPost]
        public JsonResult Assign(AssignSuveyData data)
        {
            try
            {
                foreach (var answer in data.Answers)
                {
                    var answers = answer.Answers ?? new Guid[0];
                    this.CommandService.Execute(new SetAnswerCommand(data.QuestionnaireId, answer.Id, answers.ToList(), answer.Answer, null));
                }

                this.CommandService.Execute(new ChangeAssignmentCommand(data.QuestionnaireId, data.Responsible));
                this.CommandService.Execute(
                    new ChangeStatusCommand()
                        {
                            CompleteQuestionnaireId = data.QuestionnaireId,
                            Status = SurveyStatus.Unassign,
                            Responsible = data.Responsible
                        });
            }
            catch (Exception e)
            {
                Logger.Fatal("Unexpected error occurred", e);
                return Json(new { status = "error", error = e.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
        }



        public ActionResult Summary()
        {
            ViewBag.ActivePage = MenuItem.Summary;
            return this.View(this.summaryTemplatesViewFactory.Load(new SummaryTemplatesInputModel(
                this.GlobalInfo.GetCurrentUser().Id, ViewerStatus.Headquarter)).Items);
        }

        public ActionResult Status()
        {
            ViewBag.ActivePage = MenuItem.Statuses;
            return this.View(SurveyStatusViewItems());
        }

        private DocumentFilter Filters()
        {
            var statuses = SurveyStatusViewItems();

            var viewerId = this.GlobalInfo.GetCurrentUser().Id;
            var viewerStatus = ViewerStatus.Headquarter;

            return new DocumentFilter()
            {
                Users = this.supervisorsFactory.Load(new UserListViewInputModel() { PageSize = int.MaxValue }).Items.Where(u => !u.IsLocked).Select(u => new SurveyUsersViewItem()
                    {
                        UserId = u.UserId,
                        UserName = u.UserName
                    }),
                Responsibles =
                    this.surveyUsersViewFactory.Load(new SurveyUsersViewInputModel(viewerId, viewerStatus)).Items,
                Templates =
                    this.summaryTemplatesViewFactory.Load(new SummaryTemplatesInputModel(viewerId, viewerStatus)).Items,
                Statuses = statuses
            };
        }

        private IEnumerable<SurveyStatusViewItem> SurveyStatusViewItems()
        {
            var statuses = new List<SurveyStatusViewItem>()
                {
                    new SurveyStatusViewItem()
                        {
                            StatusId = SurveyStatus.Initial.PublicId,
                            StatusName = SurveyStatus.Initial.Name
                        },
                    new SurveyStatusViewItem()
                        {
                            StatusId = SurveyStatus.Redo.PublicId,
                            StatusName = SurveyStatus.Redo.Name
                        },
                    new SurveyStatusViewItem()
                        {
                            StatusId = SurveyStatus.Complete.PublicId,
                            StatusName = SurveyStatus.Complete.Name
                        },
                    new SurveyStatusViewItem()
                        {
                            StatusId = SurveyStatus.Error.PublicId,
                            StatusName = SurveyStatus.Error.Name
                        },
                    new SurveyStatusViewItem()
                        {
                            StatusId = SurveyStatus.Approve.PublicId,
                            StatusName = SurveyStatus.Approve.Name
                        }
                };
            return statuses;
        }
    }
}