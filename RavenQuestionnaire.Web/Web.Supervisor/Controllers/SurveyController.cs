using Ncqrs;
using System;
using System.Web.Mvc;
using Web.Supervisor.Models;
using RavenQuestionnaire.Core;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using RavenQuestionnaire.Core.Views.User;
using RavenQuestionnaire.Core.Views.Survey;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;



namespace Web.Supervisor.Controllers
{
    [Authorize]
    public class SurveyController : Controller
    {
        private IViewRepository viewRepository;
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession;
        private IGlobalInfoProvider globalInfo;


        public SurveyController(IViewRepository viewRepository,
                                IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession, IGlobalInfoProvider provider)
        {
            this.viewRepository = viewRepository;
            this.documentItemSession = documentItemSession;
            this.globalInfo = provider;
        }

        public ActionResult Index()
        {
            var model = viewRepository.Load<SurveyViewInputModel, SurveyBrowseView>(new SurveyViewInputModel());
            return View(model);
        }

        public ActionResult Assigments(string id)
        {
            var user = globalInfo.GetCurrentUser();
            var users = viewRepository.Load<InterviewersInputModel, InterviewersView>(new InterviewersInputModel { Supervisor = user });
            ViewBag.Users = new SelectList(users.Items, "Id", "Login");
            var model = viewRepository.Load<SurveyGroupInputModel, SurveyGroupView>(new SurveyGroupInputModel(id));
            return View(model);
        }

        [HttpGet]
        public ActionResult Assign(string questionnaireId, string responsibleId, string responsibleName, int columnsCount)
        {
            var user = globalInfo.GetCurrentUser();
            var users = viewRepository.Load<InterviewersInputModel, InterviewersView>(new InterviewersInputModel { Supervisor = user });
            ViewBag.Users = new SelectList(users.Items, "Id", "Login");
            var model = new AssigmentModel()
                            {
                                CompleteQuestionnaireId = questionnaireId,
                                Responsible = new UserLight(responsibleId, responsibleName),
                                ColumnsCount = columnsCount
                            };
            return PartialView("EditColumn", model);
        }

        [HttpPost]
        public ActionResult Assign(string CqId, string userId)
        {
            var user = viewRepository.Load<UserViewInputModel, UserView>(new UserViewInputModel(userId));
            var responsible = (user != null) ? new UserLight(user.UserId, user.UserName) : new UserLight();
            var commandService = NcqrsEnvironment.Get<ICommandService>();
            commandService.Execute(new ChangeAssignmentCommand(Guid.Parse(CqId), responsible) );
            return Json(new { userId = responsible.Id, userName= responsible.Name, cqId=CqId }, JsonRequestBehavior.AllowGet);
        }
    }
}
