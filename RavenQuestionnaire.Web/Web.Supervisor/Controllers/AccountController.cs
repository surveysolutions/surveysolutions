using System.Web.Mvc;
using System.Web.Security;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;
using Web.Supervisor.Models;

namespace Web.Supervisor.Controllers
{
    public class AccountController : Controller
    {
        private readonly IFormsAuthentication authentication;

        public AccountController(IFormsAuthentication auth)
        {
            authentication = auth;
        }

        //
        // GET: /Account/LogOn
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

        //
        // GET: /Account/LogOff
        public ActionResult LogOff()
        {
            authentication.SignOut();

            return Redirect("~/");
        }
    }
}