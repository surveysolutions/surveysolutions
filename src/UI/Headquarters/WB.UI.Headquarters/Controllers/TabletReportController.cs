﻿using System;
using System.IO;
using System.Web.Mvc;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernel.Structures.TabletInformation;

namespace WB.UI.Headquarters.Controllers
{
    public class TabletReportController : Controller
    {
        private readonly ILogger logger;
        private readonly ITabletInformationService tabletInformationService;

        public TabletReportController(ILogger logger, ITabletInformationService tabletInformationService)
        {
            this.logger = logger;
            this.tabletInformationService = tabletInformationService;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult PostInfoPackage()
        {
            try
            {
                TabletInformationPackage tabletInformationPackage = null;
                using (var streamReader = new StreamReader(this.Request.InputStream))
                {
                    var stringData = streamReader.ReadToEnd();
                    tabletInformationPackage = JsonConvert.DeserializeObject<TabletInformationPackage>(stringData,
                        new JsonSerializerSettings
                        {
                            TypeNameHandling =
                                TypeNameHandling.Objects
                        });
                }

                this.tabletInformationService.SaveTabletInformation(tabletInformationPackage.Content,
                    tabletInformationPackage.AndroidId, tabletInformationPackage.RegistrationId);

                return this.Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                this.logger.Error(e.Message, e);
                return this.Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = "Headquarter")]
        public ActionResult Packages()
        {
            return this.View(this.tabletInformationService.GetAllTabletInformationPackages());
        }

        [Authorize(Roles = "Headquarter")]
        public ActionResult DownloadPackages(string fileName)
        {
            return this.File(this.tabletInformationService.GetFullPathToContentFile(fileName), "application/zip", fileName);
        }
    }
}