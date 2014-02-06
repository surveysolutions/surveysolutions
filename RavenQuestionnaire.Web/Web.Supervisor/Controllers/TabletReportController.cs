using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using Ionic.Zip;
using Main.Core;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.TabletInformation;
using WB.UI.Shared.Web.Filters;

namespace Web.Supervisor.Controllers
{
    public class TabletReportController : AsyncController
    {
        private readonly ILogger logger;

        public TabletReportController(ILogger logger)
        {
            this.logger = logger;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult PostInfoPackage()
        {
            try
            {
                var stringData = new StreamReader(this.Request.InputStream).ReadToEnd();
                var tabletInformationPackage = JsonConvert.DeserializeObject<TabletInformationPackage>(stringData,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling =
                            TypeNameHandling.Objects
                    });
                var dirPath = Path.Combine(AppDomain.CurrentDomain.GetData("DataDirectory").ToString(),
                          tabletInformationPackage.PackageName);
                System.IO.File.WriteAllBytes(dirPath, tabletInformationPackage.Content);
                return this.Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                return this.Json(false, JsonRequestBehavior.AllowGet);
            }
        }
    }
}