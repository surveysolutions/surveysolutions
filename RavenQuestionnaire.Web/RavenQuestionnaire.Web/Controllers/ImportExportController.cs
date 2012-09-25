using System;
using System.Web;
using System.Web.Mvc;
using Questionnaire.Core.Web.Export;
using Questionnaire.Core.Web.Threading;


namespace RavenQuestionnaire.Web.Controllers
{
   public class ImportExportController : AsyncController
   {
   
        private readonly IExportImport exportimportEvents;

        public ImportExportController(IExportImport exportImport)
        {
            this.exportimportEvents = exportImport;
        }

        #region PublicMethod

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Import()
        {
            return View("ViewTestUploadFile");
        }
       
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
       
        public ActionResult ImportCompleted()
        {
            return RedirectToAction("Index", "Dashboard");
        }
       
        public void ExportAsync(Guid clientGuid)
        {
            AsyncManager.OutstandingOperations.Increment();
            AsyncQuestionnaireUpdater.Update(() =>
            {
                try
                {
                    AsyncManager.Parameters["result"] = exportimportEvents.Export(clientGuid);
                }
                catch
                {
                    AsyncManager.Parameters["result"] = null;
                }
                AsyncManager.OutstandingOperations.Decrement();
            });
        }
       
        public ActionResult ExportCompleted(byte[] result)
        {
            return File(result, "application/zip", string.Format("backup-{0}.zip", DateTime.Now.ToString().Replace(" ", "_")));
        }

        public void DownloadAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new HttpException(404, "Invalid query string parameters.");
            AsyncManager.OutstandingOperations.Increment();
            AsyncQuestionnaireUpdater.Update(() =>
            {
                try
                {
                    AsyncManager.Parameters["result"] = exportimportEvents.ExportTemplate(id);
                }
                catch
                {
                    AsyncManager.Parameters["result"] = null;
                }
                AsyncManager.OutstandingOperations.Decrement();
            });
        }
       
        public FileResult DownloadCompleted(byte[] result)
        {
            return File(result, "application/zip", "template.zip");
        }


        #endregion
    }
}
