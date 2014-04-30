﻿using System;
using System.Web.Mvc;
using System.Web.Security;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using WB.Core.BoundedContexts.Supervisor.Users;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.SurveyManagement.Services;
using Web.Supervisor.Models;

namespace Web.Supervisor.Controllers
{
    public class AccountController : Controller
    {
        private readonly IFormsAuthentication authentication;
        private readonly IGlobalInfoProvider globalProvider;
        private readonly IPasswordHasher passwordHasher;
        private readonly IHeadquartersLoginService headquartersLoginService;
        private readonly Func<string, string, bool> validateUserCredentials;

        public AccountController(IFormsAuthentication authentication, IGlobalInfoProvider globalProvider, IPasswordHasher passwordHasher,
            IHeadquartersLoginService headquartersLoginService)
            : this(authentication, globalProvider, passwordHasher, headquartersLoginService, Membership.ValidateUser) { }

        internal AccountController(IFormsAuthentication auth, IGlobalInfoProvider globalProvider, IPasswordHasher passwordHasher,
            IHeadquartersLoginService headquartersLoginService, Func<string, string, bool> validateUserCredentials)
        {
            this.authentication = auth;
            this.globalProvider = globalProvider;
            this.passwordHasher = passwordHasher;
            this.headquartersLoginService = headquartersLoginService;
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
                if (this.LoginIncludingHeadquartersData(model.UserName, model.Password))
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

        private bool LoginIncludingHeadquartersData(string login, string password)
        {
            if (this.LoginUsingLocalDatabase(login, password))
                return true;

            this.UpdateLocalDataFromHeadquarters(login, password);

            return this.LoginUsingLocalDatabase(login, password);
        }

        private void UpdateLocalDataFromHeadquarters(string login, string password)
        {
            this.headquartersLoginService.LoginAndCreateAccount(login, password);
        }

        private bool LoginUsingLocalDatabase(string login, string password)
        {
            return this.validateUserCredentials(login, this.passwordHasher.Hash(password))
                || this.validateUserCredentials(login, SimpleHash.ComputeHash(password));
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