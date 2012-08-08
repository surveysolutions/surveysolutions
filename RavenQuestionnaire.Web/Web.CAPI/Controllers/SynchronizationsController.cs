using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using DataEntryClient.CompleteQuestionnaire;
using NLog;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web;
using Questionnaire.Core.Web.Export;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.WCF;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands.Synchronization;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Views.Synchronization;
using Web.CAPI.Utils;
using LogManager = NLog.LogManager;

namespace Web.CAPI.Controllers
{
    [AsyncTimeout(20000000)]
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

        public ActionResult Push()
        {
            return View("Spots");
        }

        #region export implementations

        public Guid? Index(string url)
        {
            Guid syncProcess = Guid.NewGuid();
            var commandService = NcqrsEnvironment.Get<ICommandService>();
            commandService.Execute(
                new CreateNewSynchronizationProcessCommand(syncProcess));

            WaitCallback callback = (state) =>
                                        {
                                            try
                                            {

                                                var process = new CompleteQuestionnaireSync(KernelLocator.Kernel,
                                                                                            syncProcess, url);
                                                process.Export();

                                            }
                                            catch (Exception e)
                                            {
                                                Logger logger = LogManager.GetCurrentClassLogger();
                                                logger.Fatal(e);
                                            }
                                        };
            ThreadPool.QueueUserWorkItem(callback, syncProcess);
            return syncProcess;

        }
        public int ProgressInPersentage(Guid id)
        {
            return
                viewRepository.Load<SyncProgressInputModel, SyncProgressView>(new SyncProgressInputModel(id)).ProgressPercentage;
        }
        public void ExportAsync()
        {
            AsyncManager.OutstandingOperations.Increment();
            AsyncQuestionnaireUpdater.Update(() =>
                                                 {
                                                     try
                                                     {
                                                         AsyncManager.Parameters["result"] =
                                                             exportimportEvents.Export(this.viewRepository);
                                                     }
                                                     catch
                                                     {
                                                         AsyncManager.Parameters["result"] = null;
                                                     }
                                                     AsyncManager.OutstandingOperations.Decrement();
                                                 });
        }

        public FileResult ExportCompleted(byte[] result)
        {
            return File(result, "application/zip",
                        string.Format("backup-{0}.zip", DateTime.Now.ToString().Replace(" ", "_")));
        }

        #endregion

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

        #region import

       
        [AcceptVerbs(HttpVerbs.Post)]
        public void ImportAsync(HttpPostedFileBase myfile)
        {
            if (myfile == null && Request.Files.Count > 0)
                myfile = Request.Files[0];
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

        #endregion

        //[Authorize]
       

    }
}
