// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImportExportController.cs" company="The World Bank">
//   Import-Export Controller
// </copyright>
// <summary>
//   The import export controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Web.Controllers
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using DataEntryClient.CompleteQuestionnaire;

    using Ionic.Zip;

    using NLog;

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
        /// Initializes a new instance of the <see cref="ImportExportController"/> class.
        /// </summary>
        public ImportExportController()
        {
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
                            var process = new UsbSyncProcess(KernelLocator.Kernel, syncProcess);

                            this.AsyncManager.Parameters["result"] = process.Export("Export DB on HQ in zip file");
                        }
                        catch (Exception e)
                        {
                            this.AsyncManager.Parameters["result"] = null;
                            Logger logger = LogManager.GetCurrentClassLogger();
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
                            var process = new TemplateExportSyncProcess(KernelLocator.Kernel, syncProcess, id, clientGuid);

                            this.AsyncManager.Parameters["result"] = process.Export("Export questionnaire template in zip file");
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
        [AcceptVerbs(HttpVerbs.Post)]
        public Guid? ImportAsync(HttpPostedFileBase uploadFile)
        {
            ZipFile zip = ZipHelper.ZipFileCheck(this.Request, uploadFile);

            if (zip == null)
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
                            var process = new UsbSyncProcess(KernelLocator.Kernel, syncProcess);

                            process.Import(zip, "Usb syncronization");
                        }
                        catch (Exception e)
                        {
                            Logger logger = LogManager.GetCurrentClassLogger();
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