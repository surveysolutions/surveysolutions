using System;
using System.Web.Mvc;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.Assign;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Json;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Views.Survey;
using RavenQuestionnaire.Core.Views.User;
using Web.Supervisor.Models;

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

        public ActionResult Index()
        {
            SurveyBrowseView model =
                viewRepository.Load<SurveyViewInputModel, SurveyBrowseView>(new SurveyViewInputModel());
            return View(model);
        }

        public ActionResult Assigments(string id)
        {
            UserLight user = globalInfo.GetCurrentUser();
            InterviewersView users =
                viewRepository.Load<InterviewersInputModel, InterviewersView>(new InterviewersInputModel { Supervisor = user });
            ViewBag.Users = new SelectList(users.Items, "Id", "Login");
            SurveyGroupView model =
                viewRepository.Load<SurveyGroupInputModel, SurveyGroupView>(new SurveyGroupInputModel(id));
            return View(model);
        }

        public ActionResult Assign(Guid id)
        {
            UserLight user = globalInfo.GetCurrentUser();
            InterviewersView users =
                viewRepository.Load<InterviewersInputModel, InterviewersView>(new InterviewersInputModel { Supervisor = user });
            ViewBag.Users = new SelectList(users.Items, "Id", "Login");
            AssignSurveyView model =
                viewRepository.Load<AssignSurveyInputModel, AssignSurveyView>(new AssignSurveyInputModel(id));
            return View(model);
        }

        [HttpGet]
        public ActionResult AssignForm(string questionnaireId, string responsibleId, string responsibleName,
                                       int columnsCount)
        {
            UserLight user = globalInfo.GetCurrentUser();
            InterviewersView users =
                viewRepository.Load<InterviewersInputModel, InterviewersView>(new InterviewersInputModel { Supervisor = user });
            ViewBag.Users = new SelectList(users.Items, "Id", "Login");
            var model = new AssigmentModel
                            {
                                CompleteQuestionnaireId = questionnaireId,
                                Responsible = new UserLight(responsibleId, responsibleName),
                                ColumnsCount = columnsCount
                            };
            return PartialView("EditColumn", model);
        }

        [HttpPost]
        public ActionResult AssignForm(string CqId, string userId)
        {
            UserView user = viewRepository.Load<UserViewInputModel, UserView>(new UserViewInputModel(userId));
            UserLight responsible = (user != null) ? new UserLight(user.UserId, user.UserName) : new UserLight();
            var commandService = NcqrsEnvironment.Get<ICommandService>();
            commandService.Execute(new ChangeAssignmentCommand(Guid.Parse(CqId), responsible));
            return Json(new { userId = responsible.Id, userName = responsible.Name, cqId = CqId },
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
                return Json(new { status = "error", question = questions[0], settings = settings[0], error = e.Message },
                        JsonRequestBehavior.AllowGet);
            }
            return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
        }
    }
}