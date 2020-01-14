using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models.Admin;
using WB.UI.Headquarters.Models.CompanyLogo;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator")]
    [ObserverNotAllowed]
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
        public IActionResult Index()
        {
            return View(new
            {
                UpdateLogoUrl = Url.Action("UpdateLogo"),
                RemoveLogoUrl = Url.Action("RemoveLogo"),
                LogoUrl = Url.Content("~/api/CompanyLogo/Thumbnail"),
                DefaultLogoUrl = Url.Content("~/img/HQ-login-3_05.png")
            });
        }

        public IActionResult EmailProviders()
        {
            return View(new EmailProviders()
            {
                Api = new
                {
                    UpdateSettings = Url.Action("UpdateEmailProviderSettings", "AdminSettings"),
                    GetSettings = Url.Action("EmailProviderSettings", "AdminSettings"),
                    SendTestEmail = Url.Action("SendTestEmail", "AdminSettings"),
                }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateLogo([FromForm(Name = "logo")]IFormFile file)
        {
            if (file?.Length > 0)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    await file.CopyToAsync(ms);
                    var array = ms.ToArray();

                    this.imageProcessingService.Validate(array);

                    this.appSettingsStorage.Store(new CompanyLogo
                    {
                        Logo = array
                    }, AppSetting.CompanyLogoStorageKey);
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveLogo()
        {
            this.appSettingsStorage.Remove(CompanyLogo.CompanyLogoStorageKey);
            return RedirectToAction("Index");
        }
    }
}
