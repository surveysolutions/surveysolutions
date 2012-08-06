using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core.Utility;
using Web.Supervisor.Models;

namespace Web.Supervisor.Controllers
{
    public class AccountController : Controller
    {
        private IFormsAuthentication authentication;

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
                    authentication.SignIn(model.UserName, false);
                    return Redirect("~/");
                }
                ModelState.AddModelError("", "The user name or password provided is incorrect.");
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
