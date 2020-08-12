﻿using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdministrationController : Controller
    {
        private readonly ITabletInformationService tabletInformationService;
        private readonly IUserViewFactory userViewFactory;
        private readonly ILogger<AdministrationController> logger;
        private readonly IAuthorizedUser authorizedUser;

        public AdministrationController(ITabletInformationService tabletInformationService, 
            IUserViewFactory userViewFactory, ILogger<AdministrationController> logger, IAuthorizedUser authorizedUser)
        {
            this.tabletInformationService = tabletInformationService;
            this.userViewFactory = userViewFactory;
            this.logger = logger;
            this.authorizedUser = authorizedUser;
        }

        [ActivePage(MenuItem.Administration_Diagnostics)]
        public IActionResult Diagnostics() => View("Index");


        [ActivePage(MenuItem.Administration_TabletInfo)]
        public IActionResult TabletInfos() => View("Index");

        [HttpPost]
        public async Task<ActionResult> TabletInfos(IFormFile file)
        {
            try
            {
                if (file != null && file.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await using (var readStream = file.OpenReadStream())
                        await readStream.CopyToAsync(ms);

                    this.tabletInformationService.SaveTabletInformation(
                        content: ms.ToArray(),
                        androidId: @"manual-restore",
                        user: this.userViewFactory.GetUser(new UserViewInputModel(this.authorizedUser.Id)));
                }
            }
            catch (Exception exception)
            {
                this.logger.LogError($"Exception on tablet info uploading: {file?.Name}", exception);
            }

            return RedirectToAction("TabletInfos");
        }
    }
}
