// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SynchronizationsController.cs" company="">
//   
// </copyright>
// <summary>
//   The synchronizations controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Web.CAPI.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Web;
    using System.Web.Mvc;

    using Core.CAPI.Views.Synchronization;

    using DataEntryClient.CompleteQuestionnaire;

    using Ionic.Zip;

    using Main.Core.Commands.Synchronization;
    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Events;
    using Main.Core.View;
    using Main.Core.View.SyncProcess;
    using Main.Core.View.User;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;

    using NLog;

    using Questionnaire.Core.Web.Export;
    using Questionnaire.Core.Web.Helpers;
    using Questionnaire.Core.Web.Threading;
    using Questionnaire.Core.Web.WCF;

    using LogManager = NLog.LogManager;

    /// <summary>
    /// The synchronizations controller.
    /// </summary>
    [AsyncTimeout(20000000)]
    public class SynchronizationsController : AsyncController
    {
        #region Constants and Fields

        /// <summary>
        /// The _global provider.
        /// </summary>
        private readonly IGlobalInfoProvider _globalProvider;

        /// <summary>
        /// The synchronizer.
        /// </summary>
        private readonly IEventSync synchronizer;

        /// <summary>
        /// The view repository.
        /// </summary>
        private readonly IViewRepository viewRepository;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizationsController"/> class.
        /// </summary>
        /// <param name="viewRepository">
        /// The view repository.
        /// </param>
        /// <param name="globalProvider">
        /// The global provider.
        /// </param>
        /// <param name="synchronizer">
        /// The synchronizer.
        /// </param>
        public SynchronizationsController(
            IViewRepository viewRepository,
            IGlobalInfoProvider globalProvider,
            IEventSync synchronizer)
        {
            this.viewRepository = viewRepository;
            this._globalProvider = globalProvider;
            this.synchronizer = synchronizer;
        }

        #endregion

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
        /// The discover async.
        /// </summary>
        public void DiscoverAsync()
        {
            UserLight user = this._globalProvider.GetCurrentUser();
            AsyncQuestionnaireUpdater.Update(AsyncManager,
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
            AsyncQuestionnaireUpdater.Update(AsyncManager,
                () =>
                {
                    byte[] file;
                    try
                    {
                        var process = new UsbSyncProcess(KernelLocator.Kernel, syncProcess);
                        file = process.Export(syncKey);
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
            return this.View(this.viewRepository.Load<SyncProgressInputModel, SyncProgressView>(new SyncProgressInputModel(id)));
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
            SyncProgressView stat = this.viewRepository.Load<SyncProgressInputModel, SyncProgressView>(new SyncProgressInputModel(id));
            if (stat == null)
            {
                return -1;
            }

            return stat.ProgressPercentage;
        }

        /// <summary>
        /// The progress partial.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// </returns>
        public ActionResult ProgressPartial(Guid id)
        {
            return this.PartialView(
                "_ProgressContent",
                this.viewRepository.Load<SyncProgressInputModel, SyncProgressView>(new SyncProgressInputModel(id)));
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
            var commandService = NcqrsEnvironment.Get<ICommandService>();
            commandService.Execute(new CreateNewSynchronizationProcessCommand(syncProcess, SynchronizationType.Pull));
            WaitCallback callback = (state) =>
                {
                    try
                    {
                        var process = new WirelessSyncProcess(KernelLocator.Kernel, syncProcess, url);
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

        /// <summary>
        /// The import async.
        /// </summary>
        /// <param name="uploadFile">
        /// Uploaded file
        /// </param>
        /// <returns>
        /// The import async.
        /// </returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public Guid? ImportAsync(HttpPostedFileBase uploadFile)
        {
            if (uploadFile == null && this.Request.Files.Count > 0)
            {
                uploadFile = this.Request.Files[0];
            }

            if (uploadFile == null || uploadFile.ContentLength == 0)
            {
                return null;
            }

            if (!ZipFile.IsZipFile(uploadFile.InputStream, false))
            {
                return null;
            }

            uploadFile.InputStream.Position = 0;

            var zip = ZipFile.Read(uploadFile.InputStream);

            Guid syncProcess = Guid.NewGuid();
            AsyncQuestionnaireUpdater.Update(AsyncManager,
                () =>
                {
                    try
                    {
                        var process = new UsbSyncProcess(KernelLocator.Kernel, syncProcess);
                        process.Import(new Guid(), zip);
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
                        var process = new WirelessSyncProcess(KernelLocator.Kernel, syncProcess, url);
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

        /// <summary>
        /// Sync process summary
        /// </summary>
        /// <returns>
        /// Json with sync process infor for current logged in user
        /// </returns>
        public JsonResult Statistics()
        {
            var user = this._globalProvider.GetCurrentUser();
            var model = this.viewRepository.Load<SyncProcessInputModel, SyncProcessView>(
                    new SyncProcessInputModel(user == null ? Guid.Empty : user.Id));
            return this.Json(model, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}