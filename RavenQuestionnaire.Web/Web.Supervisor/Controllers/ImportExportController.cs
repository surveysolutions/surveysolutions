using System;
using System.Web;
using System.Web.Mvc;
using Questionnaire.Core.Web.Export;
using Questionnaire.Core.Web.Threading;

namespace Web.Supervisor.Controllers
{
    public class ImportExportController : AsyncController
    {
        private readonly IExportImport exportimportEvents;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportExportController"/> class.
        /// </summary>
        /// <param name="exportImport">
        /// The export import.
        /// </param>
        public ImportExportController(IExportImport exportImport)
        {
            exportimportEvents = exportImport;
        }

        #region PublicMethod

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Import()
        {
            return View("ViewTestUploadFile");
        }

        /// <summary>
        /// </summary>
        /// <param name="myfile">
        /// The myfile.
        /// </param>
        [AcceptVerbs(HttpVerbs.Post)]
        public void ImportAsync(HttpPostedFileBase myfile)
        {
            if (myfile != null && myfile.ContentLength != 0)
            {
                AsyncManager.OutstandingOperations.Increment();
                AsyncQuestionnaireUpdater.Update(() =>
                                                     {
                                                         exportimportEvents.Import(myfile);
                                                         AsyncManager.OutstandingOperations.Decrement();
                                                     });
            }
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        public ActionResult ImportCompleted()
        {
            return RedirectToAction("Index", "Dashboard");
        }

        /// </summary>
        /// <param name="clientGuid">
        /// The client guid.
        /// </param>
        public void ExportAsync(Guid? clientGuid)
        {
            if (!clientGuid.HasValue || clientGuid.Value == Guid.Empty)
                return;
            

            AsyncManager.OutstandingOperations.Increment();
            AsyncQuestionnaireUpdater.Update(() =>
                                                 {
                                                     try
                                                     {
                                                         AsyncManager.Parameters["result"] =
                                                             exportimportEvents.Export(clientGuid.Value);
                                                     }
                                                     catch
                                                     {
                                                         AsyncManager.Parameters["result"] = null;
                                                     }
                                                     AsyncManager.OutstandingOperations.Decrement();
                                                 });
        }

        /// <summary>
        /// </summary>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <returns>
        /// </returns>
        public ActionResult ExportCompleted(byte[] result)
        {
            return File(result, "application/zip",
                        string.Format("backup-{0}.zip", DateTime.Now.ToString().Replace(" ", "_")));
        }

        #endregion
    }
}