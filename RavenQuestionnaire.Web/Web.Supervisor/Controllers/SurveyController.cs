using System;
using System.Linq;
using System.Web.Mvc;
using Ncqrs.Commanding.ServiceModel;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Views.Assignment;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.StatusReport;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Views.User;
using Ncqrs;


namespace Web.Supervisor.Controllers
{
    public class SurveyController : Controller
    {
        //private ICommandInvoker commandInvoker;
        private IViewRepository viewRepository;
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession;

        public SurveyController(/*ICommandInvoker commandInvoker,*/ IViewRepository viewRepository, IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession)
        {
            //this.commandInvoker = commandInvoker;
            this.viewRepository = viewRepository;
            this.documentItemSession = documentItemSession;
        }

        public ActionResult Index()
        {
            var model = viewRepository.Load<StatusReportViewInputModel, StatusReportView>(new StatusReportViewInputModel());
            ViewBag.Status = SurveyStatus.GetAllStatuses().Select(s => new StatusReportItemView(){
                StatusTitle = s.Name
            }).ToList().OrderBy(x=>x.StatusTitle); 
            return View(model);
        }
        
        public ActionResult Assigments(string id)
        {
            //change it for all featured answers
            IQueryable<CompleteQuestionnaireBrowseItem> questionnaires = documentItemSession.Query().Where(x=>x.TemplateId==id);
            var model = new AssigmentBrowseView();
            foreach (var item in questionnaires)
                model.Items.Add(new AssigmentBrowseItem(item.CompleteQuestionnaireId, item.Status, item.TemplateId, 0, item.Responsible));
            return View(model);
        }

        [HttpGet]
        public ActionResult Assign(UserBrowseInputModel input, string questionnaireId)
        {
            var users = viewRepository.Load<UserBrowseInputModel, UserBrowseView>(input);
            ViewBag.Users = new SelectList(users.Items, "Id", "UserName");
            CompleteQuestionnaireBrowseItem questionnaire = documentItemSession.Query().Where(x=>x.CompleteQuestionnaireId==questionnaireId).SingleOrDefault();
            var model = new AssigmentBrowseItem(questionnaire.CompleteQuestionnaireId, questionnaire.Status, questionnaire.TemplateId,0, questionnaire.Responsible);
            return PartialView("EditColumn", model);
        }

        [HttpPost]
        public ActionResult Assign(string Id, string userId, string Save, string Cancel)
        {
            var user = viewRepository.Load<UserViewInputModel, UserView>(new UserViewInputModel(userId));
            if (!string.IsNullOrEmpty(Save))
            {
                var commandService = NcqrsEnvironment.Get<ICommandService>();
                commandService.Execute(new ChangeAssignmentCommand(Guid.Parse(Id),
                                                                   new UserLight(user.UserId, user.UserName)));
            }
            var row = documentItemSession.Query().Where(x=>x.CompleteQuestionnaireId==Id).SingleOrDefault();
            var model = new AssigmentBrowseItem(row.CompleteQuestionnaireId, row.Status, row.TemplateId, 0, new UserLight(user.UserId, user.UserName));
            return PartialView("DisplayColumn", model);
        }

    }
}
