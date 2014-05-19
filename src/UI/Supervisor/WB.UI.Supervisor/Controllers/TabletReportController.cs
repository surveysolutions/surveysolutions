using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using Main.Core;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.TabletInformation;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Supervisor.Controllers
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

                tabletInformationService.SaveTabletInformation(tabletInformationPackage.Content,
                    tabletInformationPackage.AndroidId, tabletInformationPackage.RegistrationId);

                return this.Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                return this.Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = "Headquarter, Supervisor")]
        public ActionResult Packages()
        {
            return this.View(tabletInformationService.GetAllTabletInformationPackages());
        }

        [Authorize(Roles = "Headquarter, Supervisor")]
        public ActionResult DownloadPackages(string fileName)
        {
            return this.File(tabletInformationService.GetFullPathToContentFile(fileName), "application/zip", fileName);
        }
    }
}