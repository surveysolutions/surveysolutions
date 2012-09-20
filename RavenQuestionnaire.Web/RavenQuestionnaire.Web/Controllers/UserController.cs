using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Main.Core.View;
using Main.Core.View.User;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using RavenQuestionnaire.Core;
using Main.Core.Commands.User;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;

namespace RavenQuestionnaire.Web.Controllers
{
    //[QuestionnaireAuthorize(UserRoles.Administrator)]
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
            list.Insert(0, new UserBrowseItem(Guid.Empty, "", null, DateTime.MinValue, false, null, null));
            ViewBag.Supervisors = list;
        }
        protected void AddLocationsListToViewBag()
        {
            /*var locations =
              viewRepository.Load<LocationBrowseInputModel, LocationBrowseView>(new LocationBrowseInputModel() 
                            { PageSize = 100 }).Items;

            ViewBag.AllLocations = locations;*/
        }

        public ActionResult Index(UserBrowseInputModel input)
        {
            var model = viewRepository.Load<UserBrowseInputModel, UserBrowseView>(input);
            return View(model);
        }
        public ActionResult Manage(Guid id)
        {
            if (id == null || id == Guid.Empty)
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
                if (model.PublicKey == Guid.Empty)
                {
                    var publicKey = Guid.NewGuid();

                    if (model.Supervisor.Id != Guid.Empty )
                    {
                        var super = viewRepository.Load<UserViewInputModel, UserView>(new UserViewInputModel(model.Supervisor.Id));
                        model.Supervisor.Name = super.UserName;
                        model.Supervisor.Id = super.PublicKey;
                    }

                    var commandService = NcqrsEnvironment.Get<ICommandService>();
                    commandService.Execute(new CreateUserCommand(publicKey, model.UserName, SimpleHash.ComputeHash(model.Password), model.Email,
                        new UserRoles[] { model.PrimaryRole }, model.IsLocked, model.Supervisor));
                }
                else
                {
                    var commandService = NcqrsEnvironment.Get<ICommandService>();
                    commandService.Execute(new ChangeUserCommand(model.PublicKey, model.Email,
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
