using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Commands.User;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Location;
using RavenQuestionnaire.Core.Views.User;

namespace RavenQuestionnaire.Web.Controllers
{
    [QuestionnaireAuthorize(UserRoles.Administrator)]
    public class UserController : Controller
    {
        private IViewRepository viewRepository;
        public UserController(IViewRepository viewRepository)
        {
            
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
              viewRepository.Load<LocationBrowseInputModel, LocationBrowseView>(new LocationBrowseInputModel() 
                            { PageSize = 100 }).Items;

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
            //commandInvoker.Execute(new DeleteUserCommand(id, GlobalInfo.GetCurrentUser()));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Save(UserView model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.UserId))
                {
                    var publicKey = Guid.NewGuid();
                    
                    var commandService = NcqrsEnvironment.Get<ICommandService>();
                    commandService.Execute(new CreateUserCommand(publicKey, model.UserName, SimpleHash.ComputeHash(model.Password), model.Email,
                        new UserRoles[] { model.PrimaryRole }, model.IsLocked, model.Supervisor));
                }
                else
                {
                    var commandService = NcqrsEnvironment.Get<ICommandService>();
                    commandService.Execute(new ChangeUserCommand(Guid.Parse(model.UserId), model.Email,
                        new UserRoles[] { model.PrimaryRole }, model.IsLocked));


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
