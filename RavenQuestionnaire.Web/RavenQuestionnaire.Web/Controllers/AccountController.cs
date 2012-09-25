// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccountController.cs" company="World bank">
//   2012
// </copyright>
// <summary>
//   The account controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Web.Controllers
{
    using System;
    using System.Web.Mvc;
    using System.Web.Security;

    using Main.Core.Utility;

    using Questionnaire.Core.Web.Security;

    using RavenQuestionnaire.Web.Models;

    /// <summary>
    /// The account controller.
    /// </summary>
    public class AccountController : Controller
    {
        #region Constants and Fields

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
        public AccountController(IFormsAuthentication auth)
        {
            this.authentication = auth;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The change password.
        /// </summary>
        /// <returns>
        /// </returns>
        [Authorize]
        public ActionResult ChangePassword()
        {
            return this.View();
        }

        // POST: /Account/ChangePassword

        /// <summary>
        /// The change password.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// </returns>
        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (this.ModelState.IsValid)
            {
                // ChangePassword will throw an exception rather
                // than return false in certain failure scenarios.
                bool changePasswordSucceeded;
                try
                {
                    MembershipUser currentUser = Membership.GetUser(this.User.Identity.Name, true /* userIsOnline */);
                    changePasswordSucceeded = currentUser.ChangePassword(model.OldPassword, model.NewPassword);
                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    return this.RedirectToAction("ChangePasswordSuccess");
                }
                else
                {
                    this.ModelState.AddModelError(
                        string.Empty, "The current password is incorrect or the new password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // GET: /Account/ChangePasswordSuccess

        /// <summary>
        /// The change password success.
        /// </summary>
        /// <returns>
        /// </returns>
        public ActionResult ChangePasswordSuccess()
        {
            return this.View();
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

        // GET: /Account/LogOn

        /// <summary>
        /// The log on.
        /// </summary>
        /// <returns>
        /// </returns>
        public ActionResult LogOn()
        {
            return this.View();
        }

        // POST: /Account/LogOn

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

                    /*   if (returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {*/
                    return this.Redirect("~/");

                    // }
                }
                else
                {
                    this.ModelState.AddModelError(string.Empty, "The user name or password provided is incorrect.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        /// <summary>
        /// The log on mobile.
        /// </summary>
        /// <returns>
        /// </returns>
        public ActionResult LogOnMobile()
        {
            return this.View();
        }

        /// <summary>
        /// The log on mobile.
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
        public ActionResult LogOnMobile(LogOnModel model, string returnUrl)
        {
            if (this.ModelState.IsValid)
            {
                if (Membership.ValidateUser(model.UserName, SimpleHash.ComputeHash(model.Password)))
                {
                    this.authentication.SignIn(model.UserName, false);
                    return this.Redirect("~/");
                }
                else
                {
                    this.ModelState.AddModelError(string.Empty, "The user name or password provided is incorrect.");
                }
            }

            return View(model);
        }

        // GET: /Account/LogOff

        // GET: /Account/Register

        /// <summary>
        /// The register.
        /// </summary>
        /// <returns>
        /// </returns>
        public ActionResult Register()
        {
            return this.View();
        }

        // POST: /Account/Register

        /// <summary>
        /// The register.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// </returns>
        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            if (this.ModelState.IsValid)
            {
                // Attempt to register the user
                MembershipCreateStatus createStatus;
                Membership.CreateUser(
                    model.UserName, 
                    SimpleHash.ComputeHash(model.Password), 
                    model.Email, 
                    null, 
                    null, 
                    true, 
                    null, 
                    out createStatus);

                if (createStatus == MembershipCreateStatus.Success)
                {
                    this.authentication.SignIn(model.UserName, false /* createPersistentCookie */);
                    return this.RedirectToAction("Index", "Questionnaire");
                }
                else
                {
                    this.ModelState.AddModelError(string.Empty, ErrorCodeToString(createStatus));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion

        // GET: /Account/ChangePassword
        #region Methods

        /// <summary>
        /// The error code to string.
        /// </summary>
        /// <param name="createStatus">
        /// The create status.
        /// </param>
        /// <returns>
        /// The error code to string.
        /// </returns>
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return
                        "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return
                        "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return
                        "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return
                        "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }

        #endregion
    }
}