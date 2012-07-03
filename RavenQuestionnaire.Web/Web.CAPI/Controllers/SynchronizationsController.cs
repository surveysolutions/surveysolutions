using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Questionnaire.Core.Web;
using Questionnaire.Core.Web.Export;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.WCF;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands.Synchronization;
using RavenQuestionnaire.Core.Views.Synchronization;
using Web.CAPI.Utils;

namespace Web.CAPI.Controllers
{
    public class SynchronizationsController : AsyncController
    {
        private readonly IGlobalInfoProvider _globalProvider;
        private readonly IViewRepository viewRepository;
        private readonly IExportImport exportimportEvents ;

        public SynchronizationsController( IViewRepository viewRepository,
                                          IGlobalInfoProvider globalProvider,IExportImport exportImport)
        {
            this.exportimportEvents = exportImport;
            this.viewRepository = viewRepository;
            _globalProvider = globalProvider;
        }
       

        public ActionResult Index(string url)
        {
         /*   var user = _globalProvider.GetCurrentUser();
            
                                                     Guid syncProcess = Guid.NewGuid();
                                                     commandInvoker.Execute(
                                                         new CreateNewSynchronizationProcessCommand(syncProcess, user));
                                                     Process p = new Process();
                                                     p.StartInfo.UseShellExecute = false;
                                                     p.StartInfo.Arguments = url + " " + syncProcess;
                                                     p.StartInfo.RedirectStandardOutput = true;
                                                     p.StartInfo.FileName = System.Web.Configuration.WebConfigurationManager.AppSettings["SynchronizerPath"];
                                                     p.Start();*/
            return RedirectToAction("Progress", new {id = ""});

        }
        public ActionResult Progress(Guid id)
        {
            return View(viewRepository.Load<SyncProgressInputModel, SyncProgressView>(new SyncProgressInputModel(id)));
        }
        public ActionResult ProgressPartial(Guid id)
        {
            return PartialView("_ProgressContent",viewRepository.Load<SyncProgressInputModel, SyncProgressView>(new SyncProgressInputModel(id)));
        }

        public ActionResult DiscoverPage()
        {
            return View("Scaning");
        }

        public void DiscoverAsync()
        {
            AsyncManager.OutstandingOperations.Increment();
            var user = _globalProvider.GetCurrentUser();
            AsyncQuestionnaireUpdater.Update(() =>
            {
                try
                {
                    AsyncManager.Parameters["result"] = new ServiceDiscover().DiscoverChannels();

                }
                catch
                {
                    AsyncManager.Parameters["result"] = null;
                }
                AsyncManager.OutstandingOperations.Decrement();
            });
        }
        public ActionResult DiscoverCompleted(IEnumerable<ServiceDiscover.SyncSpot> result)
        {

            return PartialView("Spots",result.ToArray());
        }




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
            return RedirectToAction("Dashboard", "Survey");
        }
       
        public void ExportAsync()
        {
            AsyncManager.OutstandingOperations.Increment();
            AsyncQuestionnaireUpdater.Update(() =>
            {
                try
                {
                    AsyncManager.Parameters["result"] = exportimportEvents.Export();
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

    }
}
