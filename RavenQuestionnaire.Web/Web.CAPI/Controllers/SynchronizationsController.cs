using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Core.CAPI.Synchronization;
using Core.CAPI.Views.Synchronization;
using DataEntryClient.CompleteQuestionnaire;
using Main.Core.View;
using NLog;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;

using Questionnaire.Core.Web.Export;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Threading;
using Questionnaire.Core.Web.WCF;
using Main.Core.Commands.Synchronization;
using Main.Core.Documents;
using LogManager = NLog.LogManager;

namespace Web.CAPI.Controllers
{
    using Main.Core.Events;

    [AsyncTimeout(20000000)]
    public class SynchronizationsController : AsyncController
    {
        private readonly IGlobalInfoProvider _globalProvider;
        private readonly IViewRepository viewRepository;
        private readonly IExportImport exportimportEvents ;
        private readonly IEventSync synchronizer;
        public SynchronizationsController( IViewRepository viewRepository,
                                          IGlobalInfoProvider globalProvider, IExportImport exportImport, IEventSync synchronizer)
        {
            this.exportimportEvents = exportImport;
            this.viewRepository = viewRepository;
            _globalProvider = globalProvider;
            this.synchronizer = synchronizer;
        }

       

        #region export implementations

        public bool CheckIsThereSomethingToPush()
        {
            return this.synchronizer.ReadEvents().Any();
        }

        public Guid? Push(string url, Guid syncKey)
        {
            Guid syncProcess = Guid.NewGuid();
            var commandService = NcqrsEnvironment.Get<ICommandService>();
            commandService.Execute(
                new CreateNewSynchronizationProcessCommand(syncProcess,SynchronizationType.Push));

            WaitCallback callback = (state) =>
                                        {
                                            try
                                            {

                                                var process = new CompleteQuestionnaireSync(KernelLocator.Kernel,
                                                                                            syncProcess, url);
                                                process.Export(syncKey);

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

        public FileResult ExportCompleted(byte[] result)
        {
            return File(result, "application/zip",
                        string.Format("backup-{0}.zip", DateTime.Now.ToString().Replace(" ", "_")));
        }

        #endregion

        #region import

        public Guid Pull(string url, Guid syncKey)
        {
            Guid syncProcess = Guid.NewGuid();
            var commandService = NcqrsEnvironment.Get<ICommandService>();
            commandService.Execute(
                new CreateNewSynchronizationProcessCommand(syncProcess, SynchronizationType.Pull));
            WaitCallback callback = (state) =>
            {
                try
                {

                    var process = new CompleteQuestionnaireSync(KernelLocator.Kernel,
                                                                syncProcess, url);
                    process.Import(syncKey);

                }
                catch (Exception e)
                {
                    Logger logger = LogManager.GetCurrentClassLogger();
                    logger.Fatal("Error on import ", e);
                }
            };
            ThreadPool.QueueUserWorkItem(callback, syncProcess);
            return syncProcess;
        }

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
        
        #region Progress

        public int ProgressInPersentage(Guid id)
        {
            var stat = viewRepository.Load<SyncProgressInputModel, SyncProgressView>(new SyncProgressInputModel(id));
            if (stat == null)
                return -1;
            return
                stat.ProgressPercentage;
        }

        public ActionResult Progress(Guid id)
        {
            return View(viewRepository.Load<SyncProgressInputModel, SyncProgressView>(new SyncProgressInputModel(id)));
        }

        public ActionResult ProgressPartial(Guid id)
        {
            return PartialView("_ProgressContent",
                               viewRepository.Load<SyncProgressInputModel, SyncProgressView>(
                                   new SyncProgressInputModel(id)));
        }

        #endregion
        
        #region discovery

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
                                                         AsyncManager.Parameters["result"] =
                                                             new ServiceDiscover().DiscoverChannels();

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

            return PartialView("Spots", result.ToArray());
        }

        #endregion

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Import()
        {
            return View("ViewTestUploadFile");
        }
    }
}
