// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImportExportController.cs" company="">
//   2012
// </copyright>
// <summary>
//   The import export controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.Export;

namespace Web.Supervisor.Controllers
{
    using System;
    using System.Threading;
    using System.Web;
    using System.Web.Mvc;

    using Core.Supervisor.Views.SyncProcess;

    using DataEntryClient.SycProcess.Interfaces;
    using DataEntryClient.SycProcessFactory;

    using Main.Core.View;

    using NLog;

    using Questionnaire.Core.Web.Helpers;
    using Questionnaire.Core.Web.Threading;

    using Web.Supervisor.Models;

    /// <summary>
    /// The import export controller.
    /// </summary>
    [NoAsyncTimeout]
    public class ImportExportController : AsyncController
    {
        #region Constants and Fields

        /// <summary>
        /// Data exporter
        /// </summary>
        private readonly IDataExport exporter;

        /// <summary>
        /// View repository
        /// </summary>
        private readonly IViewRepository viewRepository;

        /// <summary>
        /// The syncs process factory
        /// </summary>
        private readonly ISyncProcessFactory syncProcessFactory;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportExportController"/> class.
        /// </summary>
        /// <param name="exporter">
        /// The exporter.
        /// </param>
        /// <param name="viewRepository">
        /// The view repository
        /// </param>
        /// <param name="syncProcessFactory">
        /// The sync Process Factory.
        /// </param>
        public ImportExportController(IDataExport exporter, IViewRepository viewRepository, ISyncProcessFactory syncProcessFactory)
        {
            this.exporter = exporter;
            this.viewRepository = viewRepository;
            this.syncProcessFactory = syncProcessFactory;
        }

        #endregion

        #region Public Methods and Operators

        public ActionResult Index()
        {
            ViewBag.ActivePage = MenuItem.Administration;
            var model = this.viewRepository.Load<SyncProcessLogInputModel, SyncProcessLogView>(new SyncProcessLogInputModel());
            return this.View(model);
        }


        /// <summary>
        /// The export async.
        /// </summary>
        /// <param name="syncKey">
        /// The synchronization key.
        /// </param>
        /// <returns>
        /// The sync process guid
        /// </returns>
        public Guid? ExportAsync(Guid syncKey)
        {
            Guid syncProcess = Guid.NewGuid();

            AsyncQuestionnaireUpdater.Update(
                this.AsyncManager,
                () =>
                {
                    try
                    {
                        var process = (IUsbSyncProcess)this.syncProcessFactory.GetProcess(SyncProcessType.Usb, syncProcess, null);

                        this.AsyncManager.Parameters["result"] = process.Export("Export DB on Supervisor in zip file");
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
        /// Zip archive as array of bytes
        /// </param>
        /// <returns>
        /// Downlods zip archive with events to client
        /// </returns>
        public ActionResult ExportCompleted(byte[] result)
        {
            return this.File(result, "application/zip", string.Format("backup_{0}.zip", DateTime.Now.ToString().Replace(" ", "_")));
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
        /// <param name="uploadFile">
        /// The upload File.
        /// </param>
        /// <returns>
        /// The sync process guid on null if error
        /// </returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public Guid? Import(HttpPostedFileBase uploadFile)
        {
            var zipData = ZipHelper.ZipFileReader(this.Request, uploadFile);

            if (zipData == null || zipData.Count == 0)
            {
                return null;
            }

            Guid syncProcess = Guid.NewGuid();

            WaitCallback callback = (state) =>
            {
                try
                {
                    var process = (IUsbSyncProcess)this.syncProcessFactory.GetProcess(SyncProcessType.Usb, syncProcess, null);

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
        /// The import completed 
        /// </summary>
        /// <returns>
        /// Redirects on main page after import complete
        /// </returns>
        public ActionResult ImportCompleted()
        {
            return this.RedirectToAction("Index", "Survey");
        }

        /// <summary>
        /// Gets exported data
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <exception cref="HttpException">
        /// Not found exception
        /// </exception>
        public void GetExportedDataAsync(Guid id, string type)
        {
            if ((id == null) || (id == Guid.Empty) || string.IsNullOrEmpty(type))
            {
                throw new HttpException(404, "Invalid quesry string parameters");
            }

            AsyncQuestionnaireUpdater.Update(
               this.AsyncManager,
               () =>
               {
                   try
                   {
                       this.AsyncManager.Parameters["result"] = this.exporter.ExportData(id, type);
                   }
                   catch
                   {
                       this.AsyncManager.Parameters["result"] = null;
                   }
               });
        }

        /// <summary>
        /// Gets exported data
        /// </summary>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <returns>
        /// Zipped data file
        /// </returns>
        public ActionResult GetExportedDataCompleted(byte[] result)
        {
            return this.File(
                result, "application/zip", "data.zip");
        }
        #endregion
    }
}