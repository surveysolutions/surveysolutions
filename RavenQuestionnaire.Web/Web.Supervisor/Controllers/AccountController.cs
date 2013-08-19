using System;
using System.Web.Mvc;
using System.Web.Security;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using Web.Supervisor.Models;

namespace Web.Supervisor.Controllers
{
    public class AccountController : Controller
    {
        private readonly IFormsAuthentication authentication;

        private readonly IGlobalInfoProvider globalProvider;

        public AccountController(IFormsAuthentication auth, IGlobalInfoProvider globalProvider)
        {
            this.authentication = auth;
            this.globalProvider = globalProvider;
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
                if (Membership.ValidateUser(model.UserName, SimpleHash.ComputeHash(model.Password)))
                {
                    bool isSupervisor = Roles.IsUserInRole(model.UserName, UserRoles.Supervisor.ToString());
                    bool isHeadquarter = Roles.IsUserInRole(model.UserName, UserRoles.Headquarter.ToString());
                    if (isSupervisor || isHeadquarter)
                    {
                        this.authentication.SignIn(model.UserName, false);
                        if (isSupervisor)
                        {
                            return this.RedirectToAction("Index", "Survey");
                        }
                        else
                        {
                            return this.RedirectToAction("Index", "HQ");
                        }
                    }

                    this.ModelState.AddModelError(string.Empty, "You have no access to this site. Contact your administrator.");
                }
                else
                    this.ModelState.AddModelError(string.Empty, "The user name or password provided is incorrect.");
            }

            return this.View(model);
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