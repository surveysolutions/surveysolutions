// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImportExportController.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The import export controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Web.Supervisor.Controllers
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using Questionnaire.Core.Web.Export;
    using Questionnaire.Core.Web.Threading;

    /// <summary>
    /// The import export controller.
    /// </summary>
    public class ImportExportController : AsyncController
    {
        #region Constants and Fields

        /// <summary>
        /// The exportimport events.
        /// </summary>
        private readonly IExportImport exportimportEvents;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportExportController"/> class.
        /// </summary>
        /// <param name="exportImport">
        /// The export import.
        /// </param>
        public ImportExportController(IExportImport exportImport)
        {
            this.exportimportEvents = exportImport;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The export async.
        /// </summary>
        /// <param name="clientGuid">
        /// The client guid.
        /// </param>
        public void ExportAsync(Guid syncKey)
        {
            AsyncManager.OutstandingOperations.Increment();
            AsyncQuestionnaireUpdater.Update(() =>
            {
                try
                {
                    AsyncManager.Parameters["result"] =
                        exportimportEvents.Export(syncKey);
                }
                catch
                {
                    AsyncManager.Parameters["result"] = null;
                }
                AsyncManager.OutstandingOperations.Decrement();
            });
        }

        /// <summary>
        /// The export completed.
        /// </summary>
        /// <param name="result">
        /// Zip archive as array of bytes
        /// </param>
        /// <returns>
        /// Downlods zip archive with events to client
        /// </returns>
        public ActionResult ExportCompleted(byte[] result)
        {
            return this.File(result, "application/zip", string.Format("backup-{0}.zip", DateTime.Now.ToString().Replace(" ", "_")));
        }

        /// <summary>
        /// Import action
        /// </summary>
        /// <returns>
        /// View with input to upload file
        /// </returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Import()
        {
            return this.View("ViewTestUploadFile");
        }

        /// <summary>
        /// The import async.
        /// </summary>
        /// <param name="myfile">
        /// .capi file with events
        /// </param>
        [AcceptVerbs(HttpVerbs.Post)]
        public void ImportAsync(HttpPostedFileBase myfile)
        {
            if (myfile == null && Request.Files.Count > 0)
                myfile = Request.Files[0];
            if (myfile == null || myfile.ContentLength == 0)
            {
                return;
            }

            this.AsyncManager.OutstandingOperations.Increment();
            AsyncQuestionnaireUpdater.Update(
                () =>
                    {
                        this.exportimportEvents.Import(myfile);
                        this.AsyncManager.OutstandingOperations.Decrement();
                    });
        }

        /// <summary>
        /// The import completed 
        /// </summary>
        /// <returns>
        /// Redirects on main page after import complete
        /// </returns>
        public ActionResult ImportCompleted()
        {
            return this.RedirectToAction("Index", "Dashboard");
        }

        #endregion
    }
}