using Ncqrs;
using System;
using System.Linq;
using System.Web.Mvc;
using RavenQuestionnaire.Core;
using Ncqrs.Commanding.ServiceModel;
using RavenQuestionnaire.Core.Views.User;
using RavenQuestionnaire.Core.Views.Survey;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;


namespace Web.Supervisor.Controllers
{
    public class SurveyController : Controller
    {
        private IViewRepository viewRepository;
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession;

        public SurveyController(IViewRepository viewRepository, 
                                IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession)
        {
            this.viewRepository = viewRepository;
            this.documentItemSession = documentItemSession;
        }

        public ActionResult Index()
        {
            var model = viewRepository.Load<SurveyViewInputModel, SurveyBrowseView>(new SurveyViewInputModel());
            return View(model);
        }
        
        public ActionResult Assigments(string id)
        {
            var model = viewRepository.Load<SurveyGroupInputModel, SurveyGroupView>(new SurveyGroupInputModel(id));
            return View(model);
        }

        [HttpGet]
        public ActionResult Assign(UserBrowseInputModel input, string questionnaireId)
        {
            input = new UserBrowseInputModel(UserRoles.Supervisor);
            var users = viewRepository.Load<UserBrowseInputModel, UserBrowseView>(input);
            ViewBag.Users = new SelectList(users.Items, "Id", "UserName");
            var questionnaire = documentItemSession.Query().Where(x=>x.CompleteQuestionnaireId==questionnaireId).SingleOrDefault();
            var model = viewRepository.Load<SurveyGroupInputModel, SurveyBrowseView>(new SurveyGroupInputModel(questionnaire.TemplateId, questionnaireId));
            return PartialView("EditColumn", model.Items);
        }

        [HttpPost]
        public ActionResult Assign(string Id, string userId, string Save, string Cancel)
        {
            var user = viewRepository.Load<UserViewInputModel, UserView>(new UserViewInputModel(userId));
            var responsible = (user!=null) ? new UserLight(user.UserId, user.UserName) : new UserLight();
            if (!string.IsNullOrEmpty(Save))
            {
                var commandService = NcqrsEnvironment.Get<ICommandService>();
                commandService.Execute(new ChangeAssignmentCommand() { CompleteQuestionnaireId = Guid.Parse(Id), Responsible = responsible });
            }
            var row = documentItemSession.Query().Where(x => x.CompleteQuestionnaireId == Id).SingleOrDefault();
            var model = viewRepository.Load<SurveyGroupInputModel, SurveyBrowseView>(new SurveyGroupInputModel(row.TemplateId, Id));
            return PartialView("DisplayColumn", model.Items);
        }

    }
}
