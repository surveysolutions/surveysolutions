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
using RavenQuestionnaire.Core.Views.Survey;
using RavenQuestionnaire.Core.Views.User;
using Ncqrs;


namespace Web.Supervisor.Controllers
{
    public class SurveyController : Controller
    {
        //private ICommandInvoker commandInvoker;
        private IViewRepository viewRepository;
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession;
        private readonly IDenormalizerStorage<SurveyBrowseItem> document;

        public SurveyController(ICommandInvoker commandInvoker, IViewRepository viewRepository, 
                                IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession, 
                                IDenormalizerStorage<SurveyBrowseItem> document)
        {
            //this.commandInvoker = commandInvoker;
            this.viewRepository = viewRepository;
            this.documentItemSession = documentItemSession;
            this.document = document;
        }

        public ActionResult Index()
        {
            var model = new SurveyBrowseView();
            var statuses = SurveyStatus.GetAllStatuses().Select(s =>s.Name).ToList();
            statuses.Insert(0, "UnAssignment");
            ViewBag.Status = statuses;
            var alltemplate = documentItemSession.Query().Select(x => x.TemplateId).Distinct();
            foreach (var template in alltemplate)
            {
                var title = document.Query().Where(t => t.TemplateId == template).FirstOrDefault().Title;
                var item = new SurveysBrowseItem(Guid.NewGuid(), template, title, null, null);
                foreach (var statusename in statuses)
                {
                    int count = 0;
                    count = statusename=="UnAssignment" ? document.Query().Where(t => t.TemplateId == template).Sum(x => x.UnAssignment) : document.Query().Where(t => t.TemplateId == template).Where(x=>x.Status.Name==statusename).Where(t=>t.Responsible!=null).Count();
                    item.Statistics.Add(statusename, count);
                }
                model.Items.Add(item);
            }
            return View(model);
        }
        
        public ActionResult Assigments(string id)
        {
            //change it for all featured answers
            IQueryable<CompleteQuestionnaireBrowseItem> questionnaires = documentItemSession.Query().Where(x => x.TemplateId == id);
            var model = new SurveyBrowseView();
            foreach (var item in questionnaires)
                model.Items.Add(new SurveysBrowseItem(Guid.Parse(item.CompleteQuestionnaireId), item.TemplateId, item.QuestionnaireTitle, item.Responsible, item.Status));
            return View(model);
        }

        [HttpGet]
        public ActionResult Assign(UserBrowseInputModel input, string questionnaireId)
        {
            bool c = Request.IsAjaxRequest();
            var users = viewRepository.Load<UserBrowseInputModel, UserBrowseView>(input);
            ViewBag.Users = new SelectList(users.Items, "Id", "UserName");
            var questionnaire = documentItemSession.Query().Where(x=>x.CompleteQuestionnaireId==questionnaireId).SingleOrDefault();
            var model = new SurveysBrowseItem(Guid.Parse(questionnaire.CompleteQuestionnaireId), questionnaire.TemplateId, questionnaire.QuestionnaireTitle, questionnaire.Responsible, questionnaire.Status);
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
            var model = new SurveysBrowseItem(Guid.Parse(row.CompleteQuestionnaireId), row.TemplateId, row.QuestionnaireTitle, new UserLight(user.UserId, user.UserName), row.Status);
            return PartialView("DisplayColumn", model);
        }

    }
}
