﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using Humanizer;
using Main.Core.Entities.SubEntities;
using StackExchange.Exceptional;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Diag;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.API;
using WB.UI.Headquarters.API.Filters;
using WB.UI.Headquarters.Models.Admin;
using WB.UI.Shared.Web.Filters;
using WB.UI.Shared.Web.Settings;

namespace WB.UI.Headquarters.Controllers
{
    [ControlPanelAccess]
    public class ControlPanelController : BaseController
    {
        private readonly HqUserManager userManager;
        private readonly IServiceLocator serviceLocator;
        private readonly ISettingsProvider settingsProvider;
        private readonly IPlainKeyValueStorage<ExportServiceSettings> exportServiceSettings;
        private readonly IAndroidPackageReader androidPackageReader;

        public ControlPanelController(
            IServiceLocator serviceLocator,
            ICommandService commandService,
            HqUserManager userManager,
            ILogger logger,
            ISettingsProvider settingsProvider,
            IAndroidPackageReader androidPackageReader,
            IPlainKeyValueStorage<ExportServiceSettings> exportServiceSettings)
             : base(commandService: commandService, logger: logger)
        {
            this.userManager = userManager;
            this.androidPackageReader = androidPackageReader;
            this.serviceLocator = serviceLocator;
            this.settingsProvider = settingsProvider;
            this.exportServiceSettings = exportServiceSettings;
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

        public async Task<ActionResult> ExportService()
        {
            try
            {
                var exportServiceApi = serviceLocator.GetInstance<IExportServiceApi>();
                var health = await exportServiceApi.Health();

                this.ViewData["version"] = await exportServiceApi.Version();
                this.ViewData["health"] = await health.Content.ReadAsStringAsync();
                this.ViewData["uri"] = health.RequestMessage.RequestUri.ToString();
            }
            catch (Exception e)
            {
                this.ViewData["health"] = e.ToString();
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPrivilegedUserPassword(UserEditModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await this.userManager.FindByNameAsync(model.UserName);
                var updateResult = await this.userManager.ChangePasswordAsync(user, model.Password);

                if (updateResult.Succeeded)
                {
                    this.Success($"Password for user '{user.UserName}' successfully changed");
                }
                else
                {
                    AddErrors(updateResult);
                    foreach (var error in updateResult.Errors)
                    {
                        this.Error(error, true);
                    }
                }
            }

            return this.View(model);
        }

        public ActionResult Index() => this.View();

        public ActionResult NConfig() => this.View();

        public ActionResult Versions() => this.View();

        public ActionResult AppUpdates()
        {
            var folder = Server.MapPath(ClientApkInfo.Directory);
            var appFiles = Directory.EnumerateFiles(folder);

            return View(appFiles
                .Select(app => new FileInfo(app))
                .OrderBy(fi => fi.Name)
                .Select(fi =>
                {
                    int? version = null;
                    if (fi.Name.EndsWith(".apk", StringComparison.OrdinalIgnoreCase))
                    {
                        version = this.androidPackageReader.Read(fi.FullName)?.Version;
                    }

                    return $"<strong>{fi.Name}{(version.HasValue ? $" [ver. {version}]" : "")}</strong> ({fi.Length.Bytes().ToString("0.00")}) {fi.LastWriteTimeUtc}";
                }).ToList());
        }

        public ActionResult Settings()
        {
            var model = new SettingsModel();
            model.Settings = this.settingsProvider.GetSettings().OrderBy(setting => setting.Name);
            model.ExportSettings = this.exportServiceSettings.GetById(AppSetting.ExportServiceStorageKey);

            return this.View(model);
        }

        [Localizable(false)]
        public ActionResult RepeatLastInterviewStatus(Guid? interviewId)
        {
            if (!interviewId.HasValue)
            {
                return this.View();
            }
            else
            {
                if (this.Request.Form["repeat"] != null)
                {
                    try
                    {
                        this.CommandService.Execute(new RepeatLastInterviewStatus(interviewId.Value, Strings.ControlPanelController_RepeatLastInterviewStatus));
                    }
                    catch (Exception exception)
                    {
                        Logger.Error($"Exception while repating last interview status: {interviewId}", exception);
                        return this.View(model: $"Error occurred on status repeating for interview {interviewId.Value.FormatGuid()}");
                    }

                    return this.View(model:
                        $"Successfully repeated status for interview {interviewId.Value.FormatGuid()}");
                }

                if (this.Request.Form["refreshState"] != null)
                {
                    try
                    {
                        var fixer = this.serviceLocator.GetInstance<IInterviewStateFixer>();
                        fixer.RefreshInterview(interviewId.Value);
                    }
                    catch (Exception exception)
                    {
                        Logger.Error($"Exception while refreshing interview state: {interviewId}", exception);
                        return this.View(model: $"Exception while refreshing interview state {interviewId.Value.FormatGuid()}");
                    }

                    return this.View(model: $"Successfully refreshed interview status for {interviewId.Value.FormatGuid()}");
                }
            }

            return this.View();
        }

        #region interview ravalidationg

        public class RevalidateModel
        {
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
        }

        public ActionResult RevalidateInterviews()
        {
            return this.View();
        }

        #endregion

        public ActionResult SynchronizationLog() => this.View();

        public ActionResult BrokenInterviewPackages() => this.View();
        public ActionResult RejectedInterviewPackages() => this.View();

        public Task Exceptions() => ExceptionalModule.HandleRequestAsync(System.Web.HttpContext.Current);
    }
}
