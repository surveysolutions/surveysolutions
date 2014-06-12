using System;
using System.Web.Mvc;
using System.Web.Security;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;
using Main.Core.View;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.UI.Supervisor.Models;

namespace WB.UI.Supervisor.Controllers
{
    /// <summary>
    ///     User controller responsible for dispay users, lock/unlock users, counting statistics
    /// </summary>
    [Authorize]
    public class UserController : BaseController
    {
        private readonly IFormsAuthentication authentication;
        private readonly IViewFactory<UserViewInputModel, UserView> userViewFactory;

        public UserController(
            IFormsAuthentication auth,
            ICommandService commandService,
            IGlobalInfoProvider globalInfo,
            ILogger logger,
            IViewFactory<UserViewInputModel, UserView> userViewFactory)
            : base(commandService, globalInfo, logger)
        {
            this.authentication = auth;
            this.userViewFactory = userViewFactory;
        }

        [AllowAnonymous]
        public ActionResult CreateSupervisor()
        {
            var supervisor = new SupervisorModel(Guid.NewGuid(), "supervisor") {Role = UserRoles.Headquarter};

            return this.View(supervisor);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult CreateSupervisor(SupervisorModel user)
        {
            if (this.ModelState.IsValid)
            {
                UserView userToCheck = this.userViewFactory.Load(new UserViewInputModel(UserName: user.Name, UserEmail: null));
                if (userToCheck == null)
                {
                    this.CommandService.Execute(new CreateUserCommand(user.Id, user.Name, SimpleHash.ComputeHash(user.Name),
                                                                  user.Name + "@example.com", new[] { user.Role },
                                                                  false, false, null));

                    bool isSupervisor = Roles.IsUserInRole(user.Name, UserRoles.Supervisor.ToString());
                    bool isHeadquarter = Roles.IsUserInRole(user.Name, UserRoles.Headquarter.ToString());
                    if (isSupervisor || isHeadquarter)
                    {
                        this.authentication.SignIn(user.Name, false);
                        if (isSupervisor)
                        {
                            return this.RedirectToAction("Index", "Survey");
                        }
                        else
                        {
                            return this.RedirectToAction("Index", "HQ");
                        }
                    }
                }
                else
                {
                    this.Error("User name already exists. Please enter a different user name.");
                }
            }

            return this.View(user);
        }
    }
}