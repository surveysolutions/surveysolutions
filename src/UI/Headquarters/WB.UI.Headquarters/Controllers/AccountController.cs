﻿using System;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Security;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Code.Security;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Security;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Resources;
using WB.UI.Shared.Web.Attributes;
using WB.UI.Shared.Web.Captcha;

namespace WB.UI.Headquarters.Controllers
{
    [ValidateInput(false)]
    public class AccountController : TeamController
    {
        private readonly IFormsAuthentication authentication;
        private readonly ICaptchaProvider captchaProvider;

        public AccountController(ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger,
            IFormsAuthentication authentication, IUserViewFactory userViewFactory, ICaptchaProvider captchaProvider,
            IPasswordHasher passwordHasher)
            : base(commandService, globalInfo, logger, userViewFactory, passwordHasher)
        {
            this.authentication = authentication;
            this.captchaProvider = captchaProvider;
        }

        [HttpGet]
        [Authorize]
        public ActionResult Index()
        {
            var userRoles = Roles.GetRolesForUser(this.authentication.GetCurrentUser().Name);

            bool isAdmin = userRoles.Contains(UserRoles.Administrator.ToString(), StringComparer.OrdinalIgnoreCase);
            bool isHeadquarter = userRoles.Contains(UserRoles.Headquarter.ToString(), StringComparer.OrdinalIgnoreCase);
            bool isSupervisor = userRoles.Contains(UserRoles.Supervisor.ToString(), StringComparer.OrdinalIgnoreCase);
            bool isObserver = userRoles.Contains(UserRoles.Observer.ToString(), StringComparer.OrdinalIgnoreCase);

            if (isSupervisor)
            {
                return this.RedirectToAction("Index", "Survey");
            }

            if (isHeadquarter)
            {
                return this.RedirectToAction("SurveysAndStatuses", "HQ");
            }

            if (isObserver || isAdmin)
            {
                return this.RedirectToAction("Index", "Headquarters");
            }

            return this.RedirectToAction("NotFound", "Error");
        }

        [HttpGet]
        [NoTransaction]
        public ActionResult LogOn(string returnUrl)
        {   
            this.ViewBag.ActivePage = MenuItem.Logon;
            this.ViewBag.ReturnUrl = returnUrl;

            return this.View(new LogOnModel());
        }

        [Obsolete("Replace logic with proper failed login count from membership identity as soon as it merged")]
        private bool IsRequireCaptcha(string userName)
        {
            var userKey = $@"_failedLogin_{userName}";
            var count = (int)(this.HttpContext.Cache[userKey] ?? 0);
            return count >= 5;
        }

        [Obsolete("Replace logic with proper failed login count from membership identity as soon as it merged")]
        private void RegisterFailedLogin(string userName)
        {
            var userKey = $@"_failedLogin_{userName}";
            var count = (int)(this.HttpContext.Cache[userKey] ?? 0);
            count += 1;
            HttpContext.Cache.Insert(userKey, count, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(20));
        }
        
        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            this.ViewBag.ActivePage = MenuItem.Logon;

            if (IsRequireCaptcha(model.UserName) && !this.captchaProvider.IsCaptchaValid(this))
            {
                this.ModelState.AddModelError("InvalidCaptcha", ErrorMessages.PleaseFillCaptcha);
                model.RequireCaptcha = IsRequireCaptcha(model.UserName);
                return View(model);
            }

            if (this.ModelState.IsValid && Membership.ValidateUser(model.UserName, this.passwordHasher.Hash(model.Password)))
            {
                var isInterviewer = Roles.GetRolesForUser(model.UserName).Contains(UserRoles.Operator.ToString());

                if (isInterviewer)
                    this.ModelState.AddModelError(string.Empty, ErrorMessages.SiteAccessNotAllowed);
                else
                {
                    this.authentication.SignIn(model.UserName, true);
                    return this.RedirectToLocal(returnUrl);
                }
            }
            else
            {
                RegisterFailedLogin(model.UserName);
                this.ModelState.AddModelError("InvalidCredentials", ErrorMessages.IncorrectUserNameOrPassword);
            }

            model.RequireCaptcha = IsRequireCaptcha(model.UserName);
            return this.View(model);
        }

        public ActionResult LogOff()
        {
            this.authentication.SignOut();
            return this.Redirect("~/");
        }

        public ActionResult Manage()
        {
            this.ViewBag.ActivePage = MenuItem.ManageAccount;

            var currentUser = GetUserById(GlobalInfo.GetCurrentUser().Id);

            if (currentUser == null || !(GlobalInfo.IsHeadquarter || GlobalInfo.IsAdministrator))
                throw new HttpException(404, string.Empty);

            return View(new UserEditModel() {Id = currentUser.PublicKey, Email = currentUser.Email, 
                PersonName = currentUser.PersonName, PhoneNumber = currentUser.PhoneNumber});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public ActionResult Manage(UserEditModel model)
        {
            this.ViewBag.ActivePage = MenuItem.ManageAccount;

            if (this.ModelState.IsValid)
            {
                this.UpdateAccount(user: GetUserById(GlobalInfo.GetCurrentUser().Id), editModel: model);
                this.Success(Strings.HQ_AccountController_AccountUpdatedSuccessfully);
            }
            
            return this.View(model);
        }

        
        [Authorize(Roles = "Administrator, Observer")]
        public ActionResult ObservePerson(string personName)
        {
            if (string.IsNullOrEmpty(personName)) 
                throw new HttpException(404, string.Empty);
            
            var user = Membership.GetUser(personName);
            if (user == null) 
                throw new HttpException(404, string.Empty);
            
            var currentUser = GlobalInfo.GetCurrentUser().Name;

            var forbiddenRoles = new string[] { UserRoles.Administrator.ToString(), UserRoles.Observer.ToString(), UserRoles.Operator.ToString() };
            var userRoles = Roles.GetRolesForUser(user.UserName);
            bool invalidTargetUser = userRoles.Any(r => forbiddenRoles.Contains(r));
            
            if (invalidTargetUser) 
                throw new HttpException(404, string.Empty);
            bool isHeadquarter = userRoles.Contains(UserRoles.Headquarter.ToString(), StringComparer.OrdinalIgnoreCase);
            
            //do not forget pass current user to display you are observing
            this.authentication.SignIn(user.UserName, false, currentUser);
            
            return isHeadquarter ? 
                this.RedirectToAction("SurveysAndStatuses", "HQ") : 
                this.RedirectToAction("Index", "Survey");
        }

        [Authorize(Roles = "Headquarter, Supervisor")]
        public ActionResult ReturnToObserver()
        {
            var currentUserIdentity = (User.Identity as CustomIdentity);

            if (currentUserIdentity == null || string.IsNullOrEmpty(currentUserIdentity.ObserverName))
                throw new HttpException(404, string.Empty);
            
            var alowedRoles = new string[] { UserRoles.Administrator.ToString(), UserRoles.Observer.ToString(), UserRoles.Operator.ToString() };
            var userRoles = Roles.GetRolesForUser(currentUserIdentity.ObserverName);

            bool targetUserInValidRole = userRoles.Any(r => alowedRoles.Contains(r));

            if (!targetUserInValidRole) 
                throw new HttpException(404, string.Empty);
            
            this.authentication.SignIn(currentUserIdentity.ObserverName, true);
            return this.RedirectToAction("Index", "Headquarters");
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl)) return RedirectToAction("Index");

            if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);

            return RedirectToAction("NotFound", "Error");
        }
    }
}