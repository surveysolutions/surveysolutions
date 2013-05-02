// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserController.cs" company="The World bank">
//   2012
// </copyright>
// <summary>
//  Define User controller
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Web.Http;
using System.Web.Security;
using Core.Supervisor.Views.Interviewer;
using Main.Core.Utility;
using Main.Core.View;
using Questionnaire.Core.Web.Security;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController"/> class.
        /// </summary>
        /// <param name="auth">
        /// The auth.
        /// </param>
        /// <param name="viewRepository">
        /// The view repository.
        /// </param>
        /// <param name="commandService">
        /// The command service.
        /// </param>
        /// <param name="globalInfo">
        /// The global info.
        /// </param>
        public UserController(
            IFormsAuthentication auth,
            IViewRepository viewRepository,
            ICommandService commandService,
            IGlobalInfoProvider globalInfo)
            : base(viewRepository, commandService, globalInfo)
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

        /// <summary>
        /// Unlock user
        /// </summary>
        /// <param name="id">
        /// Use public key
        /// </param>
        /// <returns>
        /// Redirects to index view if everything is ok
        /// </returns>
        public ActionResult UnlockUser(Guid id)
        {
            CommandService.Execute(new UnlockUserCommand(id));

            return this.Redirect(GlobalHelper.PreviousPage);
        }

        /// <summary>
        /// Lock user
        /// </summary>
        /// <param name="id">
        /// Use public key
        /// </param>
        /// <returns>
        /// Redirects to index view if everything is ok
        /// </returns>
        public ActionResult LockUser(Guid id)
        {
            CommandService.Execute(new LockUserCommand(id));

            return this.Redirect(GlobalHelper.PreviousPage);
        }

        /// <summary>
        /// User index page. Shows grid with supervisor's statistics grouped by interviewers
        /// </summary>
        /// <param name="input">
        /// The input model
        /// </param>
        /// <returns>
        /// Index view
        /// </returns>
        public ActionResult Index(InterviewersInputModel input)
        {
            ViewBag.ActivePage = MenuItem.Administration;
            var user = this.GlobalInfo.GetCurrentUser();
            input.ViewerId = user.Id;
            var model = this.Repository.Load<InterviewersInputModel, InterviewersView>(input);
            return this.View(model);
        }

        /// <summary>
        /// Gets table data for some view
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// Partial view with table's body
        /// </returns>
        public ActionResult _TableData(GridDataRequestModel data)
        {
            var input = new InterviewersInputModel
            {
                Page = data.Pager.Page,
                PageSize = data.Pager.PageSize,
                Orders = data.SortOrder,
                ViewerId = this.GlobalInfo.GetCurrentUser().Id
            };
            var model = this.Repository.Load<InterviewersInputModel, InterviewersView>(input);
            return this.PartialView("_PartialUsersGridTemplate", model);
        }
    }
}