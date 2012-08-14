using Ncqrs;
using System;
using System.Linq;
using System.Web.Mvc;
using Questionnaire.Core.Web.Helpers;
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
        private IGlobalInfoProvider globalInfo;

        public SurveyController(IViewRepository viewRepository, 
                                IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession,
             IGlobalInfoProvider globalInfo)
        {
            this.viewRepository = viewRepository;
            this.documentItemSession = documentItemSession;
            this.globalInfo = globalInfo;
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
            var user = globalInfo.GetCurrentUser();
            var users = viewRepository.Load<InterviewersInputModel, InterviewersView>(new InterviewersInputModel { Supervisor = user });
            ViewBag.Users = new SelectList(users.Items, "Id", "Login");
            var questionnaire = documentItemSession.Query().Where(x=>x.CompleteQuestionnaireId==questionnaireId).SingleOrDefault();
            var model = viewRepository.Load<SurveyGroupInputModel, SurveyGroupView>(new SurveyGroupInputModel(questionnaire.TemplateId, questionnaireId));
            return PartialView("EditColumn", model.Items[0]);
        }

        [HttpPost]
        public ActionResult Assign(string Id, string userId, string Save, string Cancel)
        {

            var user = viewRepository.Load<UserViewInputModel, UserView>(new UserViewInputModel(userId));
            var responsible = (user!=null) ? new UserLight(user.UserId, user.UserName) : new UserLight();
            if (!string.IsNullOrEmpty(Save))
            {
                var commandService = NcqrsEnvironment.Get<ICommandService>();
                commandService.Execute(new ChangeAssignmentCommand(Guid.Parse(Id), responsible ));
            }
            var row = documentItemSession.Query().Where(x => x.CompleteQuestionnaireId == Id).SingleOrDefault();
            var model = viewRepository.Load<SurveyGroupInputModel, SurveyGroupView>(new SurveyGroupInputModel(row.TemplateId, Id));
            return PartialView("DisplayColumn", model.Items[0]);
        }

    }
}
