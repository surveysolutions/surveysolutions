using System;
using System.Web.Mvc;
using System.Web.Security;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Models;
using WebMatrix.WebData;

namespace WB.UI.Designer.Controllers
{
    [Authorize]
    public class AccountController : AlertController
    {
        public ActionResult Index()
        {
            return this.RedirectToAction("Index", "Questionnaire");
        }
        //
        // GET: /Account/Login

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid && WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
            {
                return RedirectToLocal(returnUrl);
            }

            // If we got this far, something failed, redisplay form
            Error("The user name or password provided is incorrect.");
            return View(model);
        }

        //
        // POST: /Account/LogOff

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();

            return RedirectToAction("login", "account");
        }

        //
        // GET: /Account/Register

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Recaptcha.RecaptchaControlMvc.CaptchaValidator]
        public ActionResult Register(RegisterModel model, bool captchaValid)
        {
            if (!captchaValid)
            {
                Error("You did not type the verification word correctly. Please try again.");
            }
            else
            {
                if (ModelState.IsValid)
                {
                    // Attempt to register the user
                    try
                    {
                        string confirmationToken =
                            WebSecurity.CreateUserAndAccount(model.UserName, model.Password, new {Email = model.Email},
                                                             true);

                        if (!string.IsNullOrEmpty(confirmationToken))
                        {
                            Roles.Provider.AddUsersToRoles(new[] {model.UserName}, new[] {UserHelper.USERROLENAME});

                            dynamic email = new Postal.Email("ConfirmationEmail");
                            email.To = model.Email;
                            email.UserName = model.UserName;
                            email.ConfirmationToken = confirmationToken;
                            email.Send();

                            return RedirectToAction("registersteptwo", "account");
                        }
                    }
                    catch (MembershipCreateUserException e)
                    {
                        Error(e.StatusCode.ToErrorCode());
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult RegisterStepTwo()
        {
            return View();

        }

        [AllowAnonymous]
        public ActionResult RegisterConfirmation(string Id)
        {
            if (WebSecurity.ConfirmAccount(Id))
            {
                return RedirectToAction("confirmationsuccess");
            }
            return RedirectToAction("confirmationfailure");
        }

        [AllowAnonymous]
        public ActionResult ConfirmationSuccess()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ConfirmationFailure()
        {
            return View();
        }

        public ActionResult ExternalManage()
        {
            return RedirectToActionPermanent("manage");
        }

        //
        // GET: /Account/Manage

        public ActionResult Manage(AccountManageMessageId? message)
        {
            if (message.HasValue)
            {
                Success(message.Value.ToUIMessage());
            }
            ViewBag.ReturnUrl = Url.Action("manage");
            return View(new LocalPasswordModel());
        }

        //
        // POST: /Account/Manage

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(LocalPasswordModel model)
        {
            ViewBag.ReturnUrl = Url.Action("manage");
            if (ModelState.IsValid)
            {
                // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                bool changePasswordSucceeded;
                try
                {
                    changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword,
                                                                         model.NewPassword);
                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    //Success();
                    return RedirectToAction("manage", new {message = AccountManageMessageId.ChangePasswordSuccess});
                }
                else
                {
                    Error("The current password is incorrect or the new password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

       

        #region Helpers

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        #endregion
    }
}
