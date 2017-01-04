using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using ImageResizer;
using Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Headquarters.Models.CompanyLogo;
using WB.UI.Shared.Web.Extensions;

namespace WB.UI.Headquarters.Controllers
{
    [LimitsFilter]
    [Authorize(Roles = "Administrator")]
    [ObserverNotAllowed]
    public class SettingsController : BaseController
    {
        private readonly IPlainKeyValueStorage<CompanyLogo> logoStorage;

        public SettingsController(ICommandService commandService, 
            IGlobalInfoProvider globalInfo,
            IPlainKeyValueStorage<CompanyLogo> logoStorage,
            ILogger logger)
            : base(commandService, globalInfo, logger)
        {
            this.logoStorage = logoStorage;
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
                                ValidateImage(array);

                                this.logoStorage.Store(new CompanyLogo
                                {
                                    Logo = array
                                }, CompanyLogo.StorageKey);
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
            this.logoStorage.Remove(CompanyLogo.StorageKey);
            WriteToTempData(Alerts.SUCCESS, Settings.LogoUpdated);
            return RedirectToAction("Index");
        }

        private void ValidateImage(byte[] source)
        {
            using (var outputStream = new MemoryStream())
            {
                ImageBuilder.Current.Build(source, outputStream, new ResizeSettings
                {
                    MaxWidth = 1,
                    MaxHeight = 1,
                    Format = "png"
                });

                outputStream.ToArray();
            }
        }
    }
}