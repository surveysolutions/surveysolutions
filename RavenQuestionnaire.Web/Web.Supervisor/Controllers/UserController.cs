using System.Web.Security;
using Main.Core.Utility;
using Questionnaire.Core.Web.Security;
using WB.Core.GenericSubdomains.Logging;

namespace Web.Supervisor.Controllers
{
    using System;
    using System.Web.Mvc;

    using Main.Core.Commands.User;
    using Main.Core.Entities.SubEntities;
    using Ncqrs.Commanding.ServiceModel;
    using Questionnaire.Core.Web.Helpers;
    using Web.Supervisor.Models;

    /// <summary>
    /// User controller responsible for dispay users, lock/unlock users, counting statistics
    /// </summary>
    [Authorize]
    public class UserController : BaseController
    {
        private readonly IFormsAuthentication authentication;

        public UserController(
            IFormsAuthentication auth,
            ICommandService commandService,
            IGlobalInfoProvider globalInfo,
            ILogger logger)
            : base(commandService, globalInfo, logger)
        {
            this.authentication = auth;
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
            if (ModelState.IsValid)
            {
                CommandService.Execute(new CreateUserCommand(user.Id, user.Name, SimpleHash.ComputeHash(user.Name),
                                                             user.Name + "@worldbank.org", new[] {user.Role},
                                                             false, null));
  
                var isSupervisor = Roles.IsUserInRole(user.Name, UserRoles.Supervisor.ToString());
                var isHeadquarter = Roles.IsUserInRole(user.Name, UserRoles.Headquarter.ToString());
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

            return this.View(user);
        }
    }
}