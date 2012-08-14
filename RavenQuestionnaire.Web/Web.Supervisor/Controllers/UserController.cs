using System;
using System.Web;
using System.Web.Mvc;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands.User;
using RavenQuestionnaire.Core.Views.User;

namespace Web.Supervisor.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private IViewRepository viewRepository;
        private IGlobalInfoProvider globalInfo;

        public UserController(IViewRepository viewRepository, IGlobalInfoProvider globalInfo)
        {
            this.viewRepository = viewRepository;
            this.globalInfo = globalInfo;
        }
        //
        // GET: /User/
        public ActionResult UnlockUser(String id)
        {
            return SetUserLock(id, false);
        }
        public ActionResult LockUser(String id)
        {
            return SetUserLock(id, true);
        }

        private ActionResult SetUserLock(string id, bool status)
        {
            Guid key;
            if (!Guid.TryParse(id, out key))
                throw new HttpException("404");
            var commandService = NcqrsEnvironment.Get<ICommandService>();
            commandService.Execute(new ChangeUserStatusCommand(key, status));

            return RedirectToAction("Index");
        }

        public ActionResult Details(String id)
        {
            var input = new InterviewerInputModel(id){};
            var model = viewRepository.Load<InterviewerInputModel, InterviewerView>(input);
            return View(model);
        }

        public ActionResult Index(InterviewersInputModel input)
        {
            var user = globalInfo.GetCurrentUser();
            input.Supervisor = user;
            var model = viewRepository.Load<InterviewersInputModel, InterviewersView>(input);
            return View(model);
        }
    }
}
