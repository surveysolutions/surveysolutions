namespace Web.CAPI.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Web;
    using System.Web.Mvc;

    using Core.CAPI.Views.ExporStatistics;
    using Core.CAPI.Views.Synchronization;

    using DataEntryClient.SycProcess;
    using DataEntryClient.SycProcess.Interfaces;
    using DataEntryClient.SycProcessFactory;

    using Main.Core.Commands.Synchronization;
    using Main.Core.Documents;
    using Main.Core.Events;
    using Main.Core.View;
    using Main.Core.View.SyncProcess;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;

    using NLog;

    using Questionnaire.Core.Web.Helpers;
    using Questionnaire.Core.Web.Threading;
    using Questionnaire.Core.Web.WCF;

    using LogManager = NLog.LogManager;

    [NoAsyncTimeout]
    public class SynchronizationsController : AsyncController
    {
        private readonly IGlobalInfoProvider globalProvider;
        private readonly IEventStreamReader synchronizer;
        private readonly ISyncProcessFactory syncProcessFactory;
        private readonly IViewFactory<SyncProgressInputModel, SyncProgressView> syncProgressViewFactory;
        private readonly IViewFactory<SyncProcessInputModel, SyncProcessView> syncProcessViewFactory;
        private readonly IViewFactory<ExporStatisticsInputModel, ExportStatisticsView> exportStatisticsViewFactory;

        public SynchronizationsController(
            IGlobalInfoProvider globalProvider,
            IEventStreamReader synchronizer,
            ISyncProcessFactory syncProcessFactory,
            IViewFactory<SyncProgressInputModel, SyncProgressView> syncProgressViewFactory,
            IViewFactory<SyncProcessInputModel, SyncProcessView> syncProcessViewFactory,
            IViewFactory<ExporStatisticsInputModel, ExportStatisticsView> exportStatisticsViewFactory)
        {
            this.globalProvider = globalProvider;
            this.synchronizer = synchronizer;
            this.syncProcessFactory = syncProcessFactory;
            this.syncProgressViewFactory = syncProgressViewFactory;
            this.syncProcessViewFactory = syncProcessViewFactory;
            this.exportStatisticsViewFactory = exportStatisticsViewFactory;
        }

        #region Public Methods and Operators

        /// <summary>
        /// The check is there something to push.
        /// </summary>
        /// <returns>
        /// The check is there something to push.
        /// </returns>
        public bool CheckIsThereSomethingToPush()
        {
            return this.synchronizer.ReadEvents().Any();
        }

        /// <summary>
        /// Export process summary
        /// </summary>
        /// <returns>
        /// Json with sync process infor for current logged in user
        /// </returns>
        public JsonResult PushStatistics()
        {
            var events = this.synchronizer.ReadEvents();
            var keys = events.GroupBy(x => x.EventSourceId).Select(g => g.Key).ToList();
            var model = this.exportStatisticsViewFactory.Load(
                  new ExporStatisticsInputModel(keys));

            return this.Json(model.Items, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// The discover async.
        /// </summary>
        public void DiscoverAsync()
        {
            AsyncQuestionnaireUpdater.Update(
                this.AsyncManager,
                () =>
                {
                    try
                    {
                        this.AsyncManager.Parameters["result"] = new ServiceDiscover().DiscoverChannels();
                    }
                    catch
                    {
                        this.AsyncManager.Parameters["result"] = null;
                    }
                });
        }

        /// <summary>
        /// The discover completed.
        /// </summary>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <returns>
        /// Discover Completed view
        /// </returns>
        public ActionResult DiscoverCompleted(IEnumerable<ServiceDiscover.SyncSpot> result)
        {
            return this.PartialView("Spots", result.ToArray());
        }

        /// <summary>
        /// The discover page.
        /// </summary>
        /// <returns>
        /// Discover Page
        /// </returns>
        public ActionResult DiscoverPage()
        {
            return this.View("Scaning");
        }

        /// <summary>
        /// The export async.
        /// </summary>
        /// <param name="syncKey">
        /// The sync key.
        /// </param>
        /// <returns>
        /// The export async key.
        /// </returns>
        public Guid? ExportAsync(Guid syncKey)
        {
            Guid syncProcess = Guid.NewGuid();
            AsyncQuestionnaireUpdater.Update(
                this.AsyncManager,
                () =>
                    {
                    byte[] file;
                    try
                    {
                        var process = this.syncProcessFactory.GetUsbProcess( syncProcess);

                        file = process.Export("Export DB on CAPI in zip file");
                    }
                    catch (Exception e)
                    {
                        file = null;
                        Logger logger = LogManager.GetCurrentClassLogger();
                        logger.Fatal(e);
                    }

                    this.AsyncManager.Parameters["result"] = file;
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
        /// File for export
        /// </returns>
        public FileResult ExportCompleted(byte[] result)
        {
            return this.File(result, "application/zip", string.Format("backup-{0}.zip", DateTime.Now.ToString().Replace(" ", "_")));
        }

        /// <summary>
        /// The import.
        /// </summary>
        /// <returns>
        /// Import view
        /// </returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Import()
        {
            return this.View("ViewTestUploadFile");
        }

        /// <summary>
        /// The import completed.
        /// </summary>
        /// <returns>
        /// </returns>
        public ActionResult ImportCompleted()
        {
            return this.RedirectToAction("Dashboard", "Survey");
        }

        /// <summary>
        /// The progress.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// </returns>
        public ActionResult Progress(Guid id)
        {
            return this.View(this.syncProgressViewFactory.Load(new SyncProgressInputModel(id)));
        }

        /// <summary>
        /// The progress in persentage.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The progress in persentage.
        /// </returns>
        public int ProgressInPersentage(Guid id)
        {
            SyncProgressView stat = this.syncProgressViewFactory.Load(new SyncProgressInputModel(id));

            if (stat == null)
            {
                return -1;
            }

            return stat.ProgressPercentage;
        }

        /// <summary>
        /// The progress in persentage.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The progress in persentage.
        /// </returns>
        public bool ProgressEnding(Guid id)
        {
            var invoker = NcqrsEnvironment.Get<ICommandService>();

            try
            {
                SyncProgressView stat = this.syncProgressViewFactory.Load(new SyncProgressInputModel(id));

                if (stat == null)
                {
                    return false;
                }

                if (stat.ProgressPercentage < 100 && stat.ProgressPercentage > 0)
                {
                    invoker.Execute(new EndProcessComand(id, EventState.Error, "Process was canceled"));
                }

                return true;
            }
            catch (InvalidOperationException e)
            {
                return false;
            }
        }

        /// <summary>
        /// The pull.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <param name="syncKey">
        /// The sync key.
        /// </param>
        /// <returns>
        /// Process guid
        /// </returns>
        public Guid Pull(string url, Guid syncKey)
        {
            Guid syncProcess = Guid.NewGuid();

            WaitCallback callback = (state) =>
                {
                    try
                    {
                        var process = this.syncProcessFactory.GetNetworkProcess(syncProcess);

                        process.Import("Network syncronization", url);
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

        /// <summary>
        /// The import async.
        /// </summary>
        /// <param name="uploadFile">
        /// Uploaded file
        /// </param>
        /// <returns>
        /// The import async
        /// </returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public Guid? Import(HttpPostedFileBase uploadFile)
        {
            var zipData = ZipHelper.ZipFileReader(this.Request, uploadFile);

            if (zipData.Count == 0)
            {
                return null;
            }

            Guid syncProcess = Guid.NewGuid();

            WaitCallback callback = (state) =>
            {
                try
                {
                    var process = this.syncProcessFactory.GetUsbProcess(syncProcess);
                    process.Import(zipData, "Usb syncronization");
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



        /// <summary>
        /// The push.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <param name="syncKey">
        /// The sync key.
        /// </param>
        /// <returns>
        /// Process guid
        /// </returns>
        public Guid? Push(string url, Guid syncKey)
        {
            Guid syncProcess = Guid.NewGuid();

            WaitCallback callback = (state) =>
                {
                    try
                    {
                        var process = this.syncProcessFactory.GetNetworkProcess(syncProcess);

                        process.Export("Network export on CAPI", url);
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

        /// <summary>
        /// Sync process summary
        /// </summary>
        /// <returns>
        /// Json with sync process infor for current logged in user
        /// </returns>
        public JsonResult PullStatistics(Guid id)
        {
            var user = this.globalProvider.GetCurrentUser();
            var model = this.syncProcessViewFactory.Load(
                    new SyncProcessInputModel(id, user == null ? Guid.Empty : user.Id));
            return this.Json(model.Messages, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}