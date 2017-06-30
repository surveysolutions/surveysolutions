﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Resources;
using WB.UI.Headquarters.Services;
using WB.UI.Shared.Web.Attributes;
using WB.UI.Shared.Web.Filters;
using WB.UI.Shared.Web.Settings;

namespace WB.UI.Headquarters.Controllers
{
    [ControlPanelAccess]
    public class ControlPanelController : BaseController
    {
        private readonly IRestoreDeletedQuestionnaireProjectionsService restoreDeletedQuestionnaireProjectionsService;
        private readonly HqUserManager userManager;
        private readonly IServiceLocator serviceLocator;
        private readonly ISettingsProvider settingsProvider;

        public ControlPanelController(
            IServiceLocator serviceLocator,
            ICommandService commandService,
            HqUserManager userManager,
            ILogger logger,
            ISettingsProvider settingsProvider,
            IRestoreDeletedQuestionnaireProjectionsService restoreDeletedQuestionnaireProjectionsService)
             : base(commandService: commandService, logger: logger)
        {
            this.restoreDeletedQuestionnaireProjectionsService = restoreDeletedQuestionnaireProjectionsService;
            this.userManager = userManager;
            this.serviceLocator = serviceLocator;
            this.settingsProvider = settingsProvider;
        }

        public ActionResult CreateHeadquarters()
        {
            return this.View(new UserModel());
        }

        public ActionResult CreateAdmin()
        {
            return this.View(new UserModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateHeadquarters(UserModel model)
        {
            if (ModelState.IsValid)
            {
                var creationResult = await this.userManager.CreateUserAsync(
                            new HqUser
                            {
                                Id = Guid.NewGuid(),
                                UserName = model.UserName,
                                Email = model.Email,
                                FullName = model.PersonName,
                                PhoneNumber = model.PhoneNumber
                            }, model.Password, UserRoles.Headquarter);

                if (creationResult.Succeeded)
                {
                    this.Success(@"Headquarters successfully created");
                    return this.RedirectToAction("LogOn", "Account");
                }
                AddErrors(creationResult);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAdmin(UserModel model)
        {
            if (ModelState.IsValid)
            {
                var creationResult = await this.userManager.CreateUserAsync(
                            new HqUser
                            {
                                Id = Guid.NewGuid(),
                                UserName = model.UserName,
                                Email = model.Email,
                                FullName = model.PersonName,
                                PhoneNumber = model.PhoneNumber
                            }, model.Password, UserRoles.Administrator);

                if (creationResult.Succeeded)
                {
                    this.Success(@"Administrator successfully created");
                    return this.RedirectToAction("LogOn", "Account");
                }
                AddErrors(creationResult);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
        
        public ActionResult ResetPrivilegedUserPassword()
        {
            return this.View(new UserEditModel());
        }

        public ActionResult RestoreAllDeletedQuestionnaireProjections()
        {
            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RestoreAllDeletedQuestionnaireProjectionsPost()
        {
            this.restoreDeletedQuestionnaireProjectionsService.RestoreAllDeletedQuestionnaireProjections();
            return View("RestoreAllDeletedQuestionnaireProjections");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetUserPassword(UserEditModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await this.userManager.FindByNameAsync(model.UserName);
                var updateResult = await this.userManager.UpdateUserAsync(user, model.Password);

                if (updateResult.Succeeded)
                {
                    this.Success($"Password for user '{user.UserName}' successfully changed");
                }
                AddErrors(updateResult);
            }

            return RedirectToAction("ResetPrivilegedUserPassword");
        }

        private IRevalidateInterviewsAdministrationService RevalidateInterviewsAdministrationService
            => this.serviceLocator.GetInstance<IRevalidateInterviewsAdministrationService>();

        public ActionResult Index() => this.View();

        public ActionResult NConfig() => this.View();

        public ActionResult Versions() => this.View();

        public ActionResult Settings()
        {
            IEnumerable<ApplicationSetting> settings = this.settingsProvider.GetSettings().OrderBy(setting => setting.Name);
            return this.View(settings);
        }

        [NoTransaction]
        public ActionResult ReadSide() => this.View();

        public ActionResult RepeatLastInterviewStatus(Guid? interviewId)
        {
            if (!interviewId.HasValue)
            {
                return this.View();
            }
            else
            {
                try
                {
                    this.CommandService.Execute(new RepeatLastInterviewStatus(interviewId.Value, Strings.ControlPanelController_RepeatLastInterviewStatus));
                }
                catch (Exception exception)
                {
                    Logger.Error(string.Format("Exception while repating last interview status: {0}", interviewId), exception);
                }

                return this.View(model: string.Format("Successfully repeated status for interview {0}", interviewId.Value.FormatGuid()));
            }
        }

        #region interview ravalidationg

        public ActionResult RevalidateInterviews()
        {
            return this.View();
        }

        public ActionResult RevalidateAllInterviewsWithErrors()
        {
            this.RevalidateInterviewsAdministrationService.RevalidateAllInterviewsWithErrorsAsync();

            return this.RedirectToAction("RevalidateInterviews");
        }

        public ActionResult StopInterviewRevalidating()
        {
            this.RevalidateInterviewsAdministrationService.StopInterviewsRevalidating();

            return this.RedirectToAction("RevalidateInterviews");
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public string GetRevalidateInterviewStatus()
        {
            return this.RevalidateInterviewsAdministrationService.GetReadableStatus();
        }

        #endregion

        public ActionResult SynchronizationLog() => this.View();

        public ActionResult BrokenInterviewPackages() => this.View();
    }
}