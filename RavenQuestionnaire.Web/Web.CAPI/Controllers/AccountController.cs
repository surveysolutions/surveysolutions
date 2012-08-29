using System.Web.Mvc;
using System.Web.Security;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core.Utility;
using Web.CAPI.Models;

namespace Web.CAPI.Controllers
{
    public class AccountController : Controller
    {
        private readonly IGlobalInfoProvider _globalProvider;
        private IFormsAuthentication authentication;

        public AccountController(IFormsAuthentication auth, IGlobalInfoProvider globalProvider)
        {
            authentication = auth;
            _globalProvider = globalProvider;
        }

        //
        // GET: /Account/LogOn
        public ActionResult LogOn()
        {
            return View();
        }

        public bool IsLoggedIn()
        {
            return _globalProvider.GetCurrentUser() != null;
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
