using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Enumerator.Native.WebInterview;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models.Admin;
using WB.UI.Headquarters.Models.CompanyLogo;
using WB.UI.Shared.Web.Extensions;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class SettingsController : Controller
    {
        private readonly IPlainKeyValueStorage<CompanyLogo> appSettingsStorage;
        private readonly IImageProcessingService imageProcessingService;

        public SettingsController(IPlainKeyValueStorage<CompanyLogo> appSettingsStorage,
            IImageProcessingService imageProcessingService)
        {
            this.appSettingsStorage = appSettingsStorage;
            this.imageProcessingService = imageProcessingService;
        }

        [AntiForgeryFilter]
        [ActivePage(MenuItem.Settings)]
        public IActionResult Index()
        {
            var model = GetSettingsModel();
            return View(model);
        }

        private AdminSettingsModel GetSettingsModel()
        {
            var adminSettingsModel = new AdminSettingsModel();
            adminSettingsModel.UpdateLogoUrl = Url.Action("Index");
            adminSettingsModel.RemoveLogoUrl = Url.Action("RemoveLogo");
            adminSettingsModel.LogoUrl = Url.Content("~/api/CompanyLogo/Thumbnail");
            adminSettingsModel.DefaultLogoUrl = Url.Static("/img/HQ-login-3_05.png");
            return adminSettingsModel;
        }

        [ActivePage(MenuItem.EmailProviders)]
        public IActionResult EmailProviders()
        {
            return View(new EmailProviders()
            {
                Api = new
                {
                    UpdateSettings = Url.Action("UpdateEmailProviderSettings", "AdminSettings"),
                    GetSettings = Url.Action("EmailProviderSettings", "AdminSettings"),
                    SendTestEmail = Url.Action("SendTestEmail", "AdminSettings"),
                },
                AwsRegions = RegionEndpoint.EnumerableAllRegions.Select(s => 
                    new KeyValuePair<string,string>(s.SystemName, s.DisplayName)).ToList()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public async Task<ActionResult> UpdateLogo([FromForm(Name = "logo")]IFormFile file)
        {
            try
            {
                if (file?.Length > 0)
                {
                    using MemoryStream ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    var array = ms.ToArray();

                    this.imageProcessingService.Validate(array);

                    this.appSettingsStorage.Store(new CompanyLogo
                    {
                        Logo = array
                    }, AppSetting.CompanyLogoStorageKey);
                }

                return RedirectToAction("Index");
            }
            catch (UnknownImageFormatException)
            {
                var model = GetSettingsModel();
                model.InvalidImage = true;
                return View("Index", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveLogo()
        {
            this.appSettingsStorage.Remove(CompanyLogo.CompanyLogoStorageKey);
            return RedirectToAction("Index");
        }

        public class AdminSettingsModel
        {
            public string UpdateLogoUrl { get; set; }
            public string RemoveLogoUrl { get; set; }
            public string LogoUrl { get; set; }
            public string DefaultLogoUrl { get; set; }
            
            public bool InvalidImage { get; set; }
        }
    }
}
