﻿using System;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.UI.Designer.Models;
using WB.UI.Designer.Services;
using WB.UI.Shared.Web.Configuration;

namespace WB.UI.Designer.Controllers
{
    public class DeskController : Controller
    {
        private readonly IMembershipUserService userHelper;
        private readonly IConfigurationManager configurationManager;
        private readonly IAuthenticationService authenticationService;
        private readonly IDeskAuthenticationService deskAuthenticationService;

        public DeskController(IMembershipUserService userHelper,
            IConfigurationManager configurationManager, 
            IAuthenticationService authenticationService, 
            IDeskAuthenticationService deskAuthenticationService)
        {
            this.userHelper = userHelper;
            this.configurationManager = configurationManager;
            this.authenticationService = authenticationService;
            this.deskAuthenticationService = deskAuthenticationService;
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            if (this.userHelper.WebUser.MembershipUser != null)
                return RedirectToAction("RedirectToDesk");

            return this.View("~/Views/Account/Login.cshtml", new LogonModel
            {
                ShouldShowCaptcha = this.authenticationService.ShouldShowCaptcha()
            });
        }

        [HttpGet]
        [Authorize]
        public ActionResult RedirectToDesk()
        {
            string deskReturnUrl = deskAuthenticationService.GetReturnUrl(
                userHelper.WebUser.UserId,
                this.userHelper.WebUser.UserName,
                this.userHelper.WebUser.MembershipUser.Email, 
                DateTime.UtcNow.AddHours(24));
           
            return RedirectPermanent(deskReturnUrl);
        }
    }
}
