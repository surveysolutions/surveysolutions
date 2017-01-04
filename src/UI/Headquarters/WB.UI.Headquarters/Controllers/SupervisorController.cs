﻿using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Headquarters.Code;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.Controllers
{

    [LimitsFilter]
    [ValidateInput(false)]
    [Authorize(Roles = "Administrator, Headquarter, Observer")]
    public class SupervisorController : TeamController
    {
        public SupervisorController(ICommandService commandService, 
                              IGlobalInfoProvider globalInfo, 
                              ILogger logger,
                              IUserViewFactory userViewFactory,
                              IPasswordHasher passwordHasher,
                              IIdentityManager identityManager)
            : base(commandService, globalInfo, logger, userViewFactory, passwordHasher)
        {
            this.IdentityManager = identityManager;
        }

        protected readonly IIdentityManager IdentityManager;

        public ActionResult Create()
        {
            return this.View(new UserModel());
        }

        [HttpPost]
        [PreventDoubleSubmit]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Headquarter")]
        [ObserverNotAllowed]
        public ActionResult Create(UserModel model)
        {

            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            try
            {
                this.CreateSupervisor(model);
            }
            catch (Exception e)
            {
                this.Logger.Error(e.Message, e);
                this.Error(e.Message);
                return this.View(model);
            }
               
            this.Success(HQ.SuccessfullyCreated);
            return this.RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator, Headquarter, Observer")]
        public ActionResult Index()
        {
            return this.View();
        }

        [Authorize(Roles = "Administrator, Headquarter, Observer")]
        public ActionResult Archived()
        {
            return this.View();
        }

        [Authorize(Roles = "Administrator, Headquarter")]
        public ActionResult Edit(Guid id)
        {
            var user = this.GetUserById(id);

            if(user == null) throw new HttpException(404, string.Empty);

            return this.View(new UserEditModel()
                {
                    Id = user.PublicKey,
                    Email = user.Email,
                    IsLocked = user.IsLockedByHQ,
                    UserName = user.UserName,
                    PersonName = user.PersonName,
                    PhoneNumber = user.PhoneNumber
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Headquarter")]
        [ObserverNotAllowed]
        public ActionResult Edit(UserEditModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }
            
            var user = this.GetUserById(model.Id);
            if (user == null)
            {
                this.Error(HQ.UserNotExists);
                return this.View(model);
            }
            var forbiddenRoles = new string[] {UserRoles.Administrator.ToString(), UserRoles.Headquarter.ToString()};
            var doesUserInForbiddenRole = IdentityManager.GetRolesForUser(user.UserName).Any(r => forbiddenRoles.Contains(r));

            if (doesUserInForbiddenRole)
            {
                this.Error(HQ.NoPermission);
                return this.View(model);
            }

            this.UpdateAccount(user: user, editModel: model);
            
            this.Success(string.Format(HQ.UserWasUpdatedFormat, user.UserName));
            return this.RedirectToAction("Index");
        }
    }
}