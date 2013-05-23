// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImportExportController.cs" company="The World Bank">
//   Import-Export Controller
// </copyright>
// <summary>
//   The import export controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------


using WB.Common;
using WB.Common.Core.Logging;

namespace RavenQuestionnaire.Web.Controllers
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using DataEntryClient.SycProcess;
    using DataEntryClient.SycProcess.Interfaces;
    using DataEntryClient.SycProcessFactory;
    
    using Questionnaire.Core.Web.Helpers;
    using Questionnaire.Core.Web.Threading;

    /// <summary>
    /// The import export controller.
    /// </summary>
    [NoAsyncTimeout]
    public class ImportExportController : AsyncController
    {
        #region Constructors and Destructors

        /// <summary>
        /// The syncs process factory
        /// </summary>
        private readonly ISyncProcessFactory syncProcessFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportExportController"/> class.
        /// </summary>
        /// <param name="syncProcessFactory">
        /// The sync Process Factory.
        /// </param>
        public ImportExportController(ISyncProcessFactory syncProcessFactory)
        {
            this.syncProcessFactory = syncProcessFactory;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The export async.
        /// </summary>
        /// <param name="clientGuid">
        /// The client guid.
        /// </param>
        public Guid? ExportAsync(Guid clientGuid)
        {
            Guid syncProcess = Guid.NewGuid();

            AsyncQuestionnaireUpdater.Update(
                this.AsyncManager, 
                () =>
                    {
                        try
                        {
                            var process = (IUsbSyncProcess)this.syncProcessFactory.GetProcess(SyncProcessType.Usb, syncProcess, null);

                            this.AsyncManager.Parameters["result"] = process.Export("Export DB on HQ in zip file");
                        }
                        catch (Exception e)
                        {
                            this.AsyncManager.Parameters["result"] = null;
                            ILog logger = LogManager.GetLogger(typeof(ImportExportController));
                            logger.Fatal("Error on export ", e);
                        }
                    });
            return syncProcess;
        }

        /// <summary>
        /// The export completed.
        /// </summary>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <returns>
        /// </returns>
        public ActionResult ExportCompleted(byte[] result)
        {
            return this.File(result, "application/zip", string.Format("backup-{0}.zip", DateTime.Now.ToString().Replace(" ", "_")));
        }

        /// <summary>
        /// Started download templates
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="clientGuid">
        /// The client Guid.
        /// </param>
        public Guid? ExportTemplatesAsync(Guid? id, Guid? clientGuid)
        {
            Guid syncProcess = Guid.NewGuid();
            AsyncQuestionnaireUpdater.Update(
                this.AsyncManager, 
                () =>
                    {
                        try
                        {
                            var process = (ITemplateExportSyncProcess)this.syncProcessFactory.GetProcess(SyncProcessType.Template, syncProcess, null);

                            this.AsyncManager.Parameters["result"] = process.Export("Export questionnaire template in zip file", id, clientGuid);
                        }
                        catch
                        {
                            this.AsyncManager.Parameters["result"] = null;
                        }
                    });

            return syncProcess;
        }

        /// <summary>
        /// finish download template
        /// </summary>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <returns>
        /// return file with templates
        /// </returns>
        public FileResult ExportTemplatesCompleted(byte[] result)
        {
            return this.File(result, "application/zip", "template.zip");
        }

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

        /// <summary>
        /// The import async.
        /// </summary>
        /// <param name="uploadFile">
        /// The upload File.
        /// </param>
        /// <returns>
        /// The import async.
        /// </returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public Guid? ImportAsync(HttpPostedFileBase uploadFile)
        {
            var zipData = ZipHelper.ZipFileReader(this.Request, uploadFile);

            if (zipData == null)
            {
                return null;
            }

            Guid syncProcess = Guid.NewGuid();

            AsyncQuestionnaireUpdater.Update(
                this.AsyncManager, 
                () =>
                    {
                        try
                        {
                            var process = (IUsbSyncProcess)this.syncProcessFactory.GetProcess(SyncProcessType.Usb, syncProcess, null);
                            process.Import(zipData, "Usb syncronization");
                        }
                        catch (Exception e)
                        {
                            ILog logger = LogManager.GetLogger(typeof(ImportExportController));
                            logger.Fatal("Error on import ", e);
                        }
                    });

            return syncProcess;
        }

        /// <summary>
        /// The import completed.
        /// </summary>
        /// <returns>
        /// </returns>
        public ActionResult ImportCompleted()
        {
            return this.RedirectToAction("Index", "Questionnaire");
        }

        #endregion
    }
}