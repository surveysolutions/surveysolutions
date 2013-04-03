using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Main.Core.View;
using NLog;
using Ncqrs.Commanding.ServiceModel;

namespace WB.UI.Designer.Controllers
{
    [CustomAuthorize(Roles = "Administrator")]
    public class SynchronizationController : BaseController
    {

        
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminController"/> class.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="commandService">
        /// The command service.
        /// </param>
        public SynchronizationController(IViewRepository repository, ICommandService commandService)
            : base(repository, commandService)
        {
        }

        #endregion


        /// <summary>
        /// The import.
        /// </summary>
        /// <returns>
        /// </returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Import()
        {
            return this.View("ViewTestUploadFile");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Import(HttpPostedFileBase uploadFile)
        {
            /*var zipData = ZipFileReader(this.Request, uploadFile);

            if (zipData == null)
            {
                return this.RedirectToAction("Index", "Questionnaire");
            }

            Guid syncProcess = Guid.NewGuid();


            try
            {
                var process =
                    (IUsbSyncProcess) this.syncProcessFactory.GetProcess(SyncProcessType.Usb, syncProcess, null);
                process.Import(zipData, "Usb syncronization");
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal("Error on import ", e);
            }*/
            return this.RedirectToAction("Index", "Questionnaire");
        }

   /*     protected List<string> ZipFileReader(HttpRequestBase request, HttpPostedFileBase uploadFile)
        {
            if (uploadFile == null && request.Files.Count > 0)
            {
                uploadFile = request.Files[0];
            }

            if (uploadFile == null || uploadFile.ContentLength == 0)
            {
                return null;
            }

            return ZipManager.GetZipContent(uploadFile.InputStream);
        }*/

    }
}
