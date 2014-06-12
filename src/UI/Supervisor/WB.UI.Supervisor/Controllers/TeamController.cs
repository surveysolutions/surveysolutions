using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;
using Main.Core.View;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Supervisor.Models;

namespace WB.UI.Supervisor.Controllers
{
    public class TeamController : BaseController
    {
        private readonly IViewFactory<UserViewInputModel, UserView> userViewFactory;

        public TeamController(ICommandService commandService,
            IGlobalInfoProvider globalInfo,
            ILogger logger,
            IViewFactory<UserViewInputModel, UserView> userViewFactory)
            : base(commandService, globalInfo, logger)
        {
            this.userViewFactory = userViewFactory;
            this.ViewBag.ActivePage = MenuItem.Teams;
        }

        [Authorize(Roles = "Headquarter")]
        public ActionResult AddInterviewer(Guid id)
        {
            return this.View(new UserCreateModel() { Id = id });
        }

        [HttpPost]
        [Authorize(Roles = "Headquarter")]
        public ActionResult AddInterviewer(UserCreateModel model)
        {
            if (this.ModelState.IsValid)
            {
                UserView user =
                    this.userViewFactory.Load(
                        new UserViewInputModel(UserName: model.UserName, UserEmail: null));
                if (user == null)
                {
                    this.CommandService.Execute(new CreateUserCommand(
                        publicKey: Guid.NewGuid(),
                        userName: model.UserName,
                        password: SimpleHash.ComputeHash(model.Password),
                        email: model.Email,
                        isLockedBySupervisor: model.IsLockedBySupervisor,
                        isLockedByHQ: model.IsLockedByHeadquarters,
                        roles: new[] { UserRoles.Operator },
                        supervsor: this.GetUser(model.Id).GetUseLight()));
                    this.Success("Interviewer was successfully created");
                    return this.RedirectToAction("Interviewers", new { id = model.Id });
                }
                else
                {
                    this.Error("User name already exists. Please enter a different user name.");
                }
            }

            return this.View(model);
        }

        [Authorize(Roles = "Headquarter")]
        public ActionResult AddSupervisor()
        {
            return this.View(new UserCreateModel());
        }

        [HttpPost]
        [Authorize(Roles = "Headquarter")]
        public ActionResult AddSupervisor(UserCreateModel model)
        {
            if (this.ModelState.IsValid)
            {
                UserView user =
                    this.userViewFactory.Load(
                        new UserViewInputModel(UserName: model.UserName, UserEmail: null));
                if (user == null)
                {
                    this.CommandService.Execute(new CreateUserCommand(
                        publicKey: Guid.NewGuid(),
                        userName: model.UserName,
                        password: SimpleHash.ComputeHash(model.Password),
                        email: model.Email,
                        isLockedBySupervisor: false,
                        isLockedByHQ: model.IsLockedByHeadquarters,
                        roles: new[] { UserRoles.Supervisor },
                        supervsor: null));

                    this.Success("Supervisor was successfully created");
                    return this.RedirectToAction("Index");
                }
                else
                {
                    this.Error("User name already exists. Please enter a different user name.");
                }
            }

            return this.View(model);
        }

        [Authorize(Roles = "Headquarter")]
        public ActionResult Index()
        {
            return this.View();
        }

        [Authorize(Roles = "Headquarter, Supervisor")]
        public ActionResult Interviewers(Guid? id)
        {
            if (this.GlobalInfo.IsHeadquarter && !id.HasValue)
                return this.RedirectToAction("Index");

            return this.View(id);
        }

        private UserView GetUser(Guid id)
        {
            return this.userViewFactory.Load(new UserViewInputModel(id));
        }

        [Authorize(Roles = "Headquarter, Supervisor")]
        public ActionResult Details(Guid id)
        {
            var user = this.GetUser(id);

            if (user == null)
                throw new HttpException(404, string.Empty);

            bool isHeadquarter = Roles.IsUserInRole(this.GlobalInfo.GetCurrentUser().Name, UserRoles.Headquarter.ToString());

            bool isChangingUserSupervisor = Roles.IsUserInRole(user.UserName, UserRoles.Supervisor.ToString());

            UserViewModel model;

            if (isHeadquarter)
            {
                model = isChangingUserSupervisor
                    ? new SupervisorUserViewModel()
                    {
                        Id = user.PublicKey,
                        Email = user.Email,
                        IsLockedByHeadquarters = user.IsLockedByHQ,
                        IsLockedBySupervisor = user.IsLockedBySupervisor,
                        UserName = user.UserName
                    }
                    : new UserViewModel()
                    {
                        Id = user.PublicKey,
                        Email = user.Email,
                        IsLockedByHeadquarters = user.IsLockedByHQ,
                        IsLockedBySupervisor = user.IsLockedBySupervisor,
                        UserName = user.UserName
                    };
            }
            else
                model = new UserViewModelForSupervisor()
                {
                    Id = user.PublicKey,
                    Email = user.Email,
                    IsLockedByHeadquarters = user.IsLockedByHQ,
                    IsLockedBySupervisor = user.IsLockedBySupervisor,
                    UserName = user.UserName
                };

            return this.View(model);
        }

        [Authorize(Roles = "Headquarter, Supervisor")]
        [HttpPost]
        public ActionResult Details(UserViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var user = this.GetUser(model.Id);
                if (user != null)
                {
                    bool isHeadquarter = Roles.IsUserInRole(this.GlobalInfo.GetCurrentUser().Name, UserRoles.Headquarter.ToString());
                    //check for intruders
                    var updatedLockByHQ = isHeadquarter ? model.IsLockedByHeadquarters : user.IsLockedByHQ;

                    this.CommandService.Execute(
                        new ChangeUserCommand(
                            user.PublicKey,
                            model.Email,
                            user.Roles.ToArray(),
                            model.IsLockedBySupervisor,
                            updatedLockByHQ,
                            string.IsNullOrEmpty(model.Password)
                                ? user.Password
                                : SimpleHash.ComputeHash(model.Password),
                            this.GlobalInfo.GetCurrentUser().Id
                            ));
                    this.Success(string.Format("Information about <b>{0}</b> successfully updated", user.UserName));
                    return this.DetailsBackByUser(user);
                }
                else
                {
                    this.Error("Could not update user information because current user does not exist");
                }
            }

            return this.View(model);
        }

        [Authorize(Roles = "Headquarter, Supervisor")]
        public ActionResult DetailsBack(Guid id)
        {
            var user = this.GetUser(id);
            return this.DetailsBackByUser(user);
        }

        private ActionResult DetailsBackByUser(UserView user)
        {
            return this.GlobalInfo.IsHeadquarter && user.Supervisor == null
                ? this.RedirectToAction("Index")
                : this.RedirectToAction("Interviewers", new { id = user.Supervisor.Id });
        }
    }
}