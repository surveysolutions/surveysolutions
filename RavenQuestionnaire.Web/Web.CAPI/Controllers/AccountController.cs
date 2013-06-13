// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccountController.cs" company="">
//   2012
// </copyright>
// <summary>
//   The account controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Web.CAPI.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Security;

    using Main.Core.Entities.SubEntities;
    using Main.Core.Events;
    using Main.Core.Utility;

    using Questionnaire.Core.Web.Helpers;
    using Questionnaire.Core.Web.Security;

    using Web.CAPI.Models;

    /// <summary>
    /// The account controller.
    /// </summary>
    public class AccountController : Controller
    {
        #region Constants and Fields

        /// <summary>
        /// The _global provider.
        /// </summary>
        private readonly IGlobalInfoProvider globalProvider;


        /// <summary>
        /// The authentication.
        /// </summary>
        private readonly IFormsAuthentication authentication;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="auth">
        /// The auth.
        /// </param>
        /// <param name="globalProvider">
        /// The global provider.
        /// </param>
        /// <param name="userEventSync">
        /// The user event sync.
        /// </param>
        public AccountController(
            IFormsAuthentication auth, IGlobalInfoProvider globalProvider)
        {
            this.authentication = auth;
            this.globalProvider = globalProvider;
        }

        #endregion

        // GET: /Account/LogOn
        #region Public Methods and Operators

        /// <summary>
        /// The get current user.
        /// </summary>
        /// <returns>
        /// The <see cref="Guid"/>.
        /// </returns>
        public Guid GetCurrentUser()
        {
            UserLight currentUser = this.globalProvider.GetCurrentUser();
            return currentUser != null ? currentUser.Id : Guid.Empty;
        }

        /// <summary>
        /// The is logged in.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool IsLoggedIn()
        {
            return this.globalProvider.GetCurrentUser() != null;
        }

        /// <summary>
        /// The log off.
        /// </summary>
        /// <returns>
        /// </returns>
        public ActionResult LogOff()
        {
            this.authentication.SignOut();
            return this.Redirect("~/");
        }

        /// <summary>
        /// The log on.
        /// </summary>
        /// <returns>
        /// </returns>
        public ActionResult LogOn()
        {
            return this.View();
        }

        /// <summary>
        /// The log on.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="returnUrl">
        /// The return url.
        /// </param>
        /// <returns>
        /// </returns>
        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (this.ModelState.IsValid)
            {
                if (Membership.ValidateUser(model.UserName, SimpleHash.ComputeHash(model.Password)))
                {
                    this.authentication.SignIn(model.UserName, false);
                    return this.Redirect("~/");
                }

                this.ModelState.AddModelError(string.Empty, "The user name or password provided is incorrect.");
            }

            return View(model);
        }

        #endregion
    }
}