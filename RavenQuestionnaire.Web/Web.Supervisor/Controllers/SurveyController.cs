using Ncqrs;
using System;
using System.Web;
using System.Web.Mvc;
using RavenQuestionnaire.Core.Views.Statistics;
using Web.Supervisor.Models;
using RavenQuestionnaire.Core;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using RavenQuestionnaire.Core.Views.User;
using RavenQuestionnaire.Core.Views.Assign;
using RavenQuestionnaire.Core.Views.Survey;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile;


namespace Web.Supervisor.Controllers
{
    [Authorize]
    public class SurveyController : Controller
    {
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession;
        private readonly IGlobalInfoProvider globalInfo;
        private readonly IViewRepository viewRepository;


        public SurveyController(IViewRepository viewRepository,
                                IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession,
                                IGlobalInfoProvider provider)
        {
            this.viewRepository = viewRepository;
            this.documentItemSession = documentItemSession;
            globalInfo = provider;
        }

        public ActionResult Index(SurveyViewInputModel input)
        {
            SurveyBrowseView model = viewRepository.Load<SurveyViewInputModel, SurveyBrowseView>(input);
            return View(model);
        }

        public ActionResult Assigments(string id, SurveyGroupInputModel input)
        {
            var inputModel = input==null ? new SurveyGroupInputModel(){ Id = id } : new SurveyGroupInputModel(id, input.Page, input.PageSize, input.Orders);
            var user = globalInfo.GetCurrentUser();
            var users = viewRepository.Load<InterviewersInputModel, InterviewersView>(new InterviewersInputModel
                                                                                  {Supervisor = user});
            ViewBag.Users = new SelectList(users.Items, "Id", "Login");
            SurveyGroupView model = viewRepository.Load<SurveyGroupInputModel, SurveyGroupView>(inputModel);
            return View(model);
        }

        public ActionResult Assign(Guid id)
        {
            UserLight user = globalInfo.GetCurrentUser();
            InterviewersView users = viewRepository.Load<InterviewersInputModel, InterviewersView>(new InterviewersInputModel
                                                                                  {Supervisor = user});
            ViewBag.Users = new SelectList(users.Items, "Id", "Login");
            AssignSurveyView model = viewRepository.Load<AssignSurveyInputModel, AssignSurveyView>(new AssignSurveyInputModel(id));
            return View(model);
        }

        public ActionResult Approve(Guid id,string template)
        {
            var stat = viewRepository.Load<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
                    new CompleteQuestionnaireStatisticViewInputModel(id.ToString()));
            return View(new ApproveModel(){Id = id, Statistic = stat, TemplateId = template});
        }

        [HttpPost]
        public ActionResult Approve(ApproveModel model)
        {
            if (ModelState.IsValid)
            {
                var commandService = NcqrsEnvironment.Get<ICommandService>();
                var status = SurveyStatus.Approve;
                status.ChangeComment = model.Comment;
                commandService.Execute(new ChangeStatusCommand() { CompleteQuestionnaireId = model.Id, Status = status });
                return RedirectToAction("Assigments", new { id = model.TemplateId});
            }
            else
            {
                var stat = viewRepository.Load
                    <CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
                        new CompleteQuestionnaireStatisticViewInputModel(model.Id.ToString()));
                return View(new ApproveModel() {Id = model.Id, Statistic = stat, TemplateId = model.TemplateId});
            }
        }

        public ActionResult Details(Guid id,string template, Guid? group, Guid? question,  Guid? propagationKey)
        {
            //if (id)
            //    throw new HttpException(404, "Invalid query string parameters");
            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireMobileView>(
                new CompleteQuestionnaireViewInputModel(id) { CurrentGroupPublicKey = group,  PropagationKey = propagationKey });
            ViewBag.CurrentQuestion = question.HasValue ? question.Value : new Guid();
            ViewBag.TemplateId = template;
            return View(model);
        }

        public PartialViewResult Screen(Guid id, Guid group, Guid? propagationKey)
        {
            //if (string.IsNullOrEmpty(id))
            //    throw new HttpException(404, "Invalid query string parameters");
            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteGroupMobileView>(
                new CompleteQuestionnaireViewInputModel(id, group, propagationKey));
            ViewBag.CurrentQuestion = new Guid();
            ViewBag.PagePrefix = "";
            return PartialView("_SurveyContent", model);
        }

        [HttpPost]
        public ActionResult AssignForm(string CqId, string userId)
        {
            UserView user = viewRepository.Load<UserViewInputModel, UserView>(new UserViewInputModel(userId));
            UserLight responsible = (user != null) ? new UserLight(user.UserId, user.UserName) : new UserLight();
            var commandService = NcqrsEnvironment.Get<ICommandService>();
            commandService.Execute(new ChangeAssignmentCommand(Guid.Parse(CqId), responsible));
            return Json(new {userId = responsible.Id, userName = responsible.Name, cqId = CqId},
                        JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveAnswer(CompleteQuestionSettings[] settings, CompleteQuestionView[] questions)
        {
            var question = questions[0];
            try
            {
                var commandService = NcqrsEnvironment.Get<ICommandService>();
                commandService.Execute(new SetAnswerCommand(Guid.Parse(settings[0].QuestionnaireId), question,
                                                            settings[0].PropogationPublicKey));
            }
            catch (Exception e)
            {
                NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Fatal(e);
                return Json(new {status = "error", question = questions[0], settings = settings[0], error = e.Message},
                            JsonRequestBehavior.AllowGet);
            }
            return Json(new {status = "ok"}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _TableData(GridDataRequestModel data)
        {
            var input = new SurveyViewInputModel
                            {
                                Page = data.Pager.Page,
                                PageSize = data.Pager.PageSize,
                                Orders = data.SortOrder
                            };
            var model = viewRepository.Load<SurveyViewInputModel, SurveyBrowseView>(input);
            return PartialView("_Table", model);
        }

        public ActionResult _GroupTableData(GridDataRequestModel data)
        {
            UserLight user = globalInfo.GetCurrentUser();
            var users =
                viewRepository.Load<InterviewersInputModel, InterviewersView>(new InterviewersInputModel { Supervisor = user });
            ViewBag.Users = new SelectList(users.Items, "Id", "Login");
            var input = new SurveyGroupInputModel(data.Id, data.Pager.Page, data.Pager.PageSize, data.SortOrder);
            var model = viewRepository.Load<SurveyGroupInputModel, SurveyGroupView>(input);
            return PartialView("_TableGroup", model);
        }
    }
}