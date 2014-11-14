﻿using System;
using System.Web;
using System.Web.Mvc;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Headquarter")]
    public class SupervisorController : TeamController
    {
        public SupervisorController(ICommandService commandService, 
                              IGlobalInfoProvider globalInfo, 
                              ILogger logger,
                              IViewFactory<UserViewInputModel, UserView> userViewFactory,
                              IPasswordHasher passwordHasher)
            : base(commandService, globalInfo, logger, userViewFactory, passwordHasher)
        {
            
        }

        public ActionResult Create()
        {
            return this.View(new UserModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserModel model)
        {
            if (this.ModelState.IsValid)
            {
                UserView user = GetUserByName(model.UserName);
                if (user == null)
                {
                    this.CreateSupervisor(model);
                    this.Success("Supervisor was successfully created");
                    return this.RedirectToAction("Index");
                }
                else
                {
                    this.Error("User name already exists. Please enter a different user name.");
                }
            }

            return this.View(model);
        }

        
        public ActionResult Index()
        {
            return this.View();
        }

        public ActionResult Edit(Guid? id)
        {
            if (!id.HasValue)
                return this.RedirectToAction("Edit", "Supervisor", new {id = GlobalInfo.GetCurrentUser().Id});

            var user = this.GetUserById(id.Value);

            if(user == null) throw new HttpException(404, string.Empty);

            return this.View(new UserEditModel()
                {
                    Id = user.PublicKey,
                    Email = user.Email,
                    IsLocked = user.IsLockedByHQ,
                    UserName = user.UserName
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserEditModel model)
        {
            if (this.ModelState.IsValid)
            {
                var user = this.GetUserById(model.Id);
                if (user != null)
                {
                    this.UpdateSupervisorOrInterviewer(user: user, editModel: model);
                    this.Success(string.Format("Information about <b>{0}</b> successfully updated", user.UserName));
                    return this.RedirectToAction("Index");
                }
                else
                {
                    this.Error("Could not update user information because current user does not exist");
                }
            }

            return this.View(model);
        }

        public ActionResult Interviewers(Guid id)
        {
            var supervisor = this.GetUserById(id);
            if (supervisor == null)
                throw new HttpException(404, string.Empty);

            return this.View(supervisor);
        }
    }
}