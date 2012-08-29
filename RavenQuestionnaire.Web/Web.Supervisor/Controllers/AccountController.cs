using System.Web.Mvc;
using System.Web.Security;
using Web.Supervisor.Models;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Entities.SubEntities;


namespace Web.Supervisor.Controllers
{
    public class AccountController : Controller
    {
        private readonly IFormsAuthentication authentication;
        private readonly IGlobalInfoProvider globalProvider;

        public AccountController(IFormsAuthentication auth, IGlobalInfoProvider _globalProvider)
        {
            authentication = auth;
            globalProvider = _globalProvider;
        }

        [HttpGet]
        public ActionResult LogOn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (Membership.ValidateUser(model.UserName, SimpleHash.ComputeHash(model.Password)))
                {
                    if (Roles.IsUserInRole(model.UserName, UserRoles.Supervisor.ToString()) ||
                        Roles.IsUserInRole(model.UserName, UserRoles.Administrator.ToString()))
                    {
                        authentication.SignIn(model.UserName, false);
                        return Redirect("~/");
                    }
                    ModelState.AddModelError("", "You have no access to this site. Contact your administrator.");
                }
                else
                {
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                }
            }
            return View(model);
        }

        public bool IsLoggedIn()
        {
            return globalProvider.GetCurrentUser() != null;
        }

        public ActionResult LogOff()
        {
            authentication.SignOut();
            return Redirect("~/");
        }
    }
}