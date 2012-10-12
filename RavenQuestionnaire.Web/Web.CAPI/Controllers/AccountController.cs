using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Main.Core.Events;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using Main.Core.Utility;
using Web.CAPI.Models;

namespace Web.CAPI.Controllers
{
    public class AccountController : Controller
    {
        private readonly IGlobalInfoProvider _globalProvider;
        private IFormsAuthentication authentication;
        private IUserEventSync _userEventSync;

        public AccountController(IFormsAuthentication auth, IGlobalInfoProvider globalProvider, IUserEventSync userEventSync)
        {
            authentication = auth;
            _globalProvider = globalProvider;
            _userEventSync = userEventSync;
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

        public bool IsUserInBase()
        {
            var count = _userEventSync.GetUsers(Main.Core.Entities.SubEntities.UserRoles.User);
            if (count == null) return false;
            return count.ToList().Count > 0;
        }
    }
}
