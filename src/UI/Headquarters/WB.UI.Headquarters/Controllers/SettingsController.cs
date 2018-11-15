using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using ImageResizer;
using Resources;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models.CompanyLogo;
using WB.UI.Shared.Web.Extensions;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Headquarters.Controllers
{
    [LimitsFilter]
    [AuthorizeOr403(Roles = "Administrator")]
    [ObserverNotAllowed]
    public class SettingsController : BaseController
    {
        private readonly IPlainKeyValueStorage<CompanyLogo> appSettingsStorage;
        private readonly IImageProcessingService imageProcessingService;

        public SettingsController(ICommandService commandService,
            IPlainKeyValueStorage<CompanyLogo> appSettingsStorage,
            IImageProcessingService imageProcessingService,
            ILogger logger)
            : base(commandService, logger)
        {
            this.appSettingsStorage = appSettingsStorage;
            this.imageProcessingService = imageProcessingService;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> UpdateLogo()
        {
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];

                if (file != null && file.ContentLength > 0)
                {
                    using (var fileInputStream = file.InputStream)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            await fileInputStream.CopyToAsync(ms);
                            var array = ms.ToArray();

                            try
                            {
                                this.imageProcessingService.ValidateImage(array);

                                this.appSettingsStorage.Store(new CompanyLogo
                                {
                                    Logo = array
                                }, CompanyLogo.CompanyLogoStorageKey);
                                WriteToTempData(Alerts.SUCCESS, Settings.LogoUpdated);
                            }
                            catch (ImageCorruptedException)
                            {
                                WriteToTempData(Alerts.ERROR, Settings.LogoNotUpdated);
                            }
                        }
                    }
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult RemoveLogo()
        {
            this.appSettingsStorage.Remove(CompanyLogo.CompanyLogoStorageKey);
            WriteToTempData(Alerts.SUCCESS, Settings.LogoUpdated);
            return RedirectToAction("Index");
        }
    }
}
