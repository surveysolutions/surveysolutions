// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccountController.cs" company="World bank">
//   2012
// </copyright>
// <summary>
//   Defines the AccountController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Web.Supervisor.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Security;

    using Main.Core.Entities.SubEntities;
    using Main.Core.Events;
    using Main.Core.Utility;

    using Questionnaire.Core.Web.Helpers;
    using Questionnaire.Core.Web.Security;

    using Web.Supervisor.Models;

    /// <summary>
    /// AccountController responsible for users authentication
    /// </summary>
    public class AccountController : Controller
    {
        #region Fields

        /// <summary>
        /// Authentication object
        /// </summary>
        private readonly IFormsAuthentication authentication;

        /// <summary>
        /// Global info object
        /// </summary>
        private readonly IGlobalInfoProvider globalProvider;

        /// <summary>
        /// Users info
        /// </summary>
        private IUserEventSync _userEventSync;

        #endregion

        #region Constructror

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="auth">
        /// The auth.
        /// </param>
        /// <param name="globalProvider">
        /// The global provider.
        /// </param>
        public AccountController(IFormsAuthentication auth, IGlobalInfoProvider globalProvider, IUserEventSync userEventSync)
        {
            this.authentication = auth;
            this.globalProvider = globalProvider;
            this._userEventSync = userEventSync;
        }

        #endregion

        #region Actions

        /// <summary>
        /// Show LogOn Page
        /// </summary>
        /// <returns>
        /// LogOn Page
        /// </returns>
        [HttpGet]
        public ActionResult LogOn()
        {
            ViewBag.ActivePage = MenuItem.Logon;
            return this.View();
        }

        /// <summary>
        /// Redirect on needed page after authentication if everything is Ok
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="returnUrl">
        /// The return url.
        /// </param>
        /// <returns>
        /// Redirect on page after authentication
        /// </returns>
        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            ViewBag.ActivePage = MenuItem.Logon;
            if (ModelState.IsValid)
            {
                if (Membership.ValidateUser(model.UserName, SimpleHash.ComputeHash(model.Password)))
                {
                    var isSupervisor = Roles.IsUserInRole(model.UserName, UserRoles.Supervisor.ToString());
                    var isHeadquarter = Roles.IsUserInRole(model.UserName, UserRoles.Headquarter.ToString());
                    if (isSupervisor || isHeadquarter)
                    {
                        this.authentication.SignIn(model.UserName, false);
                        if (isSupervisor)
                            return this.Redirect("~/");
                        else
                            return this.RedirectToRoute("HeadquarterDashboard");
                    }

                    ModelState.AddModelError(string.Empty, "You have no access to this site. Contact your administrator.");
                }
                else 
                    ModelState.AddModelError(string.Empty, "The user name or password provided is incorrect.");
            }

            return this.View(model);
        }

        /// <summary>
        /// Check if loggedIn
        /// </summary>
        /// <returns>
        /// result user
        /// </returns>
        public bool IsLoggedIn()
        {
            return this.globalProvider.GetCurrentUser() != null;
        }

        /// <summary>
        /// Logout user
        /// </summary>
        /// <returns>
        /// Return Login page
        /// </returns>
        public ActionResult LogOff()
        {
            this.authentication.SignOut();
            return this.Redirect("~/");
        }

        /// <summary>
        /// Count of available users in database
        /// </summary>
        /// <returns>whether users</returns>
        public bool IsUserInBase()
        {
            var count = _userEventSync.GetUsers(UserRoles.Supervisor);
            if (count == null) return false;
            return count.ToList().Count > 0;

        }

        /// <summary>
        /// Generate guid of current user
        /// </summary>
        /// <returns>
        /// Guid of current user
        /// </returns>
        public Guid GetCurrentUser()
        {
            var currentUser = this.globalProvider.GetCurrentUser();
            return currentUser != null ? currentUser.Id : Guid.Empty;
        }

        #endregion
    }
}