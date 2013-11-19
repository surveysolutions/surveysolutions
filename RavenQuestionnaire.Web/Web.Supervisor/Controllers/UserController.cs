using System;
using System.Web.Mvc;
using System.Web.Security;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using Web.Supervisor.Models;

namespace Web.Supervisor.Controllers
{
    /// <summary>
    ///     User controller responsible for dispay users, lock/unlock users, counting statistics
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
            if (this.ModelState.IsValid)
            {
                this.CommandService.Execute(new CreateUserCommand(user.Id, user.Name, SimpleHash.ComputeHash(user.Name),
                                                                  user.Name + "@worldbank.org", new[] {user.Role},
                                                                  false, null));

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

            return this.View(user);
        }
    }
}