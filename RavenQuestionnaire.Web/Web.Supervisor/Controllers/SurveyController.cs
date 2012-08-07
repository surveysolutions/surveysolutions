using System.Linq;
using System.Web.Mvc;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Views.Assignment;
using RavenQuestionnaire.Core.Views.StatusReport;
using RavenQuestionnaire.Core.Entities.SubEntities;


namespace Web.Supervisor.Controllers
{
    public class SurveyController : Controller
    {

        private ICommandInvoker commandInvoker;
        private IViewRepository viewRepository;

        public SurveyController(ICommandInvoker commandInvoker, IViewRepository viewRepository)
        {
            this.commandInvoker = commandInvoker;
            this.viewRepository = viewRepository;
        }

        public ActionResult Index()
        {
            //var model = viewRepository.Load<StatusReportViewInputModel, StatusReportGroupView>(new StatusReportViewInputModel());
            var model = viewRepository.Load<StatusReportViewInputModel, StatusReportView>(new StatusReportViewInputModel());
            ViewBag.Status = SurveyStatus.GetAllStatuses().Select(s => new StatusReportItemView()
            {
                StatusTitle = s.Name
            }).ToList().OrderBy(x=>x.StatusTitle); 
            return View(model);
        }
        
        public ActionResult Assigments(string id)
        {
            var model = viewRepository.Load<AssigmentViewInputModel, AssigmentBrowseView>(new AssigmentViewInputModel(id));
            return View(model);
        }

    }
}
