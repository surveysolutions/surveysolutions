using System;
using System.Web;
using System.Web.Mvc;
using RavenQuestionnaire.Core;
using Questionnaire.Core.Web.Export;

namespace Web.CAPI.Controllers
{
    public class ImportExportController : AsyncController
    {

        private readonly IExportImport exportimportEvents;
        private readonly IViewRepository viewRepository;

        public ImportExportController(IExportImport exportImport, IViewRepository viewRepository)
        {
            this.exportimportEvents = exportImport;
            this.viewRepository = viewRepository;
        }

        #region PublicMethod
        

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
            return RedirectToAction("Dashboard", "Survey");
        }

        public void ExportAsync()
        {
            AsyncManager.OutstandingOperations.Increment();
            AsyncQuestionnaireUpdater.Update(() =>
            {
                try
                {
                    AsyncManager.Parameters["result"] = exportimportEvents.Export(this.viewRepository);
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

        #endregion

    }
}
