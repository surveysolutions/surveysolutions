using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Questionnaire.Core.Web.Helpers;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands.User;
using RavenQuestionnaire.Core.Views.User;

namespace Web.Supervisor.Controllers
{
    public class UserController : Controller
    {
        private ICommandInvoker commandInvoker;
        private IViewRepository viewRepository;

        public UserController(ICommandInvoker commandInvoker, IViewRepository viewRepository)
        {
            this.commandInvoker = commandInvoker;
            this.viewRepository = viewRepository;
        }
        //
        // GET: /User/
        public ActionResult UnlockUser(String id)
        {
            commandInvoker.Execute(new ChangeUserStatusCommand(id, false, GlobalInfo.GetCurrentUser()));
            return RedirectToAction("Index");
        }
        public ActionResult LockUser(String id)
        {
            commandInvoker.Execute(new ChangeUserStatusCommand(id, true, GlobalInfo.GetCurrentUser()));
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
            var user = GlobalInfo.GetCurrentUser();
            input.Supervisor = user;
            var model = viewRepository.Load<InterviewersInputModel, InterviewersView>(input);
            return View(model);
        }

        public ActionResult All(InterviewersInputModel input)
        {
            var user = GlobalInfo.GetCurrentUser();
            input.Supervisor = user;
            input.AllSubordinateUsers = true;
            var model = viewRepository.Load<InterviewersInputModel, InterviewersView>(input);
            return View(model);
        }
    }
}
