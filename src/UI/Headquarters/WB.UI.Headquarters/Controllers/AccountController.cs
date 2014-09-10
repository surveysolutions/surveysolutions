using System;
using System.Web.Mvc;
using System.Web.Security;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Security;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Controllers
{
    public class AccountController : Controller
    {
        private readonly IFormsAuthentication authentication;
        private readonly IGlobalInfoProvider globalProvider;
        private readonly IPasswordHasher passwordHasher;
        private readonly Func<string, string, bool> validateUserCredentials;

        public AccountController(IFormsAuthentication authentication, IGlobalInfoProvider globalProvider, IPasswordHasher passwordHasher)
            : this(authentication, globalProvider, passwordHasher, Membership.ValidateUser) { }

        internal AccountController(IFormsAuthentication auth, IGlobalInfoProvider globalProvider, IPasswordHasher passwordHasher, Func<string, string, bool> validateUserCredentials)
        {
            this.authentication = auth;
            this.globalProvider = globalProvider;
            this.passwordHasher = passwordHasher;
            this.validateUserCredentials = validateUserCredentials;
        }

        [HttpGet]
        public ActionResult LogOn()
        {
            this.ViewBag.ActivePage = MenuItem.Logon;
            return this.View();
        }

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            this.ViewBag.ActivePage = MenuItem.Logon;
            if (this.ModelState.IsValid)
            {
                if (this.Login(model.UserName, model.Password))
                {
                    bool isHeadquarter = Roles.IsUserInRole(model.UserName, UserRoles.Headquarter.ToString());
                    bool isSupervisor = Roles.IsUserInRole(model.UserName, UserRoles.Supervisor.ToString());
                    if (isHeadquarter || (isSupervisor && LegacyOptions.SupervisorFunctionsEnabled))
                    {
                        this.authentication.SignIn(model.UserName, false);
                        if (isSupervisor)
                        {
                            return this.RedirectToAction("Index", "Survey");
                        }
                        else
                        {
                            return this.RedirectToAction("SurveysAndStatuses", "HQ");
                        }
                    }

                    this.ModelState.AddModelError(string.Empty, "You have no access to this site. Contact your administrator.");
                }
                else
                    this.ModelState.AddModelError(string.Empty, "The user name or password provided is incorrect.");
            }

            return this.View(model);
        }

        private bool Login(string login, string password)
        {
            return this.validateUserCredentials(login, this.passwordHasher.Hash(password)) || this.validateUserCredentials(login, SimpleHash.ComputeHash(password));
        }

        public bool IsLoggedIn()
        {
            return this.globalProvider.GetCurrentUser() != null;
        }

        public ActionResult LogOff()
        {
            this.authentication.SignOut();
            return this.Redirect("~/");
        }

        public Guid GetCurrentUser()
        {
            UserLight currentUser = this.globalProvider.GetCurrentUser();
            return currentUser != null ? currentUser.Id : Guid.Empty;
        }
    }
}