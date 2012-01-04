using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Location;
using RavenQuestionnaire.Core.Views.User;

namespace RavenQuestionnaire.Web.Controllers
{
    [QuestionnaireAuthorize(UserRoles.Administrator)]
    public class UserController : Controller
    {
        private ICommandInvoker commandInvoker;
        private IViewRepository viewRepository;
        public UserController(ICommandInvoker commandInvoker, IViewRepository viewRepository)
        {
            this.commandInvoker = commandInvoker;
            this.viewRepository = viewRepository;
        }
        protected void AddSupervisorListToViewBag()
        {
            var supervisors =
                viewRepository.Load<UserBrowseInputModel, UserBrowseView>(new UserBrowseInputModel(UserRoles.Supervisor)
                                                                              {PageSize = 100}).Items;
            List<UserBrowseItem> list = supervisors.ToList();
            list.Insert(0, new UserBrowseItem("", "", null, DateTime.MinValue, false, null, null));
            ViewBag.Supervisors = list;
        }
        protected void AddLocationsListToViewBag()
        {
            var locations =
              viewRepository.Load<LocationBrowseInputModel, LocationBrowseView>(new LocationBrowseInputModel() { PageSize = 100 }).Items;

            ViewBag.AllLocations = locations;
        }

        public ActionResult Index(UserBrowseInputModel input)
        {
            var model = viewRepository.Load<UserBrowseInputModel, UserBrowseView>(input);
            return View(model);
        }
        public ActionResult Manage(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid quesry string parameters");
            var model = viewRepository.Load<UserViewInputModel, UserView>(new UserViewInputModel(id));
            AddSupervisorListToViewBag();
            AddLocationsListToViewBag();
            return View(model);
        }
        public ActionResult Delete(string id)
        {
            commandInvoker.Execute(new DeleteUserCommand(id, GlobalInfo.GetCurrentUser()));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Save(UserView model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.UserId))
                {
                    commandInvoker.Execute(new CreateNewUserCommand(model.UserName, model.Email,SimpleHash.ComputeHash(model.Password),
                                                                    model.PrimaryRole, model.IsLocked, model.SupervisorId, model.LocationId,
                                                                    GlobalInfo.GetCurrentUser()));
                }
                else
                {
                    commandInvoker.Execute(new UpdateUserCommand(model.UserId, model.Email, model.IsLocked,
                                                                 new UserRoles[]
                                                                     {
                                                                         model.PrimaryRole
                                                                     }, model.SupervisorId, model.LocationId,
                                                                     GlobalInfo.GetCurrentUser()));
                }
                return RedirectToAction("Index");

            }
            return View("Manage", model);
        }
        public ActionResult Create()
        {
            AddSupervisorListToViewBag();
            AddLocationsListToViewBag();
            return View("Manage", UserView.New());
        }
    }
}
