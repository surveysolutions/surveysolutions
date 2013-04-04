// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImportExportController.cs" company="">
//   
// </copyright>
// <summary>
//   The import export controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Web.Supervisor.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Web;
    using System.Web.Mvc;

    using Core.Supervisor.Views.SyncProcess;

    using DataEntryClient.SycProcess.Interfaces;
    using DataEntryClient.SycProcessFactory;

    using Main.Core.Export;
    using Main.Core.View;
    using Main.Synchronization.SyncManager;
    using Main.Synchronization.SyncSreamProvider;
    using Main.Synchronization.SyncStreamCollector;

    using NLog;

    using Newtonsoft.Json;

    using Questionnaire.Core.Web.Helpers;
    using Questionnaire.Core.Web.Threading;

    using SynchronizationMessages.CompleteQuestionnaire;

    using Web.Supervisor.Models;
    using Web.Supervisor.Utils.Attributes;

    /// <summary>
    /// The import export controller.
    /// </summary>
    [NoAsyncTimeout]
    public class ImportExportController : AsyncController
    {
        #region Fields

        /// <summary>
        /// Data exporter
        /// </summary>
        private readonly IDataExport exporter;

        /// <summary>
        /// The syncs process factory
        /// </summary>
        private readonly ISyncProcessFactory syncProcessFactory;

        /// <summary>
        /// View repository
        /// </summary>
        private readonly IViewRepository viewRepository;

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
        public ImportExportController(
            IDataExport exporter, IViewRepository viewRepository, ISyncProcessFactory syncProcessFactory)
        {
            this.exporter = exporter;
            this.viewRepository = viewRepository;
            this.syncProcessFactory = syncProcessFactory;
        }

        #endregion

        #region Public Methods and Operators

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

                        this.AsyncManager.Parameters["result"] =
                            process.Export("Export DB on Supervisor in zip file");
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
        /// The backup async.
        /// </summary>
        /// <param name="syncKey">
        /// The sync key.
        /// </param>
        /// <returns>
        /// The <see cref="Guid?"/>.
        /// </returns>
        public Guid? BackupAsync(Guid syncKey)
        {
            Guid syncProcessKey = Guid.NewGuid();

            AsyncQuestionnaireUpdater.Update(
                this.AsyncManager,
                () =>
                {
                    try
                    {
                        var syncResult = new Main.Synchronization.SyncManager.SyncronizationStatus();
                        var collector = new CompressedStreamStreamCollector(syncProcessKey);

                        var syncManager = new SyncManager(new AllIntEventsStreamProvider(), 
                            collector, 
                            syncProcessKey, 
                            "Backup Request", 
                            null,
                            syncResult);

                        syncManager.StartPump();

                        var timings = syncManager.StartTime - syncManager.EndTime;

                        this.AsyncManager.Parameters["result"] = collector.GetExportedStream();
                    }
                    catch (Exception e)
                    {
                        this.AsyncManager.Parameters["result"] = null;
                        Logger logger = LogManager.GetCurrentClassLogger();
                        logger.Fatal("Error on export " + e.Message, e);
                        if (e.InnerException != null)
                        {
                            logger.Fatal("Error on export (Inner Exception)", e.InnerException);
                        }
                    }
                });

            return syncProcessKey;
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
        public ActionResult BackupCompleted(Stream result)
        {
            if (result != null)
            {
                return this.File(
                    result,
                    "application/zip",
                    string.Format("backup_{0}.zip", DateTime.UtcNow.ToString("yyyyMMddhhnnss")));
            }

            return null;
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
            return this.File(result, "application/zip", "data.zip");
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
            List<string> zipData = ZipHelper.ZipFileReader(this.Request, uploadFile);

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
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            this.ViewBag.ActivePage = MenuItem.Administration;
            SyncProcessLogView model =
                this.viewRepository.Load<SyncProcessLogInputModel, SyncProcessLogView>(new SyncProcessLogInputModel());
            return this.View(model);
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
            /*   SyncProgressView stat = this.viewRepository.Load<SyncProgressInputModel, SyncProgressView>(new SyncProgressInputModel(id));

            if (stat == null)
            {
                return -1;
            }

            return stat.ProgressPercentage;*/
            return 100;
        }

        private ListOfAggregateRootsForImportMessage GetList()
        {
            Guid syncProcess = Guid.NewGuid();

            var result = new ListOfAggregateRootsForImportMessage();

            try
            {
                var process = (IEventSyncProcess)this.syncProcessFactory.GetProcess(SyncProcessType.Event, syncProcess, null);

                result = process.Export("Supervisor export AR events");
            }
            catch (Exception ex)
            {
            }

            return result;
        }



        private SyncItemsMetaContainer GetListOfAR()
        {
            Guid syncProcess = Guid.NewGuid();

            var result = new SyncItemsMetaContainer();

            try
            {
                var process = (IEventSyncProcess)this.syncProcessFactory.GetProcess(SyncProcessType.Event, syncProcess, null);

                result = process.GetListOfAggregateRoots("Supervisor export AR events");
            }
            catch (Exception ex)
            {
            }

            return result;
        }



        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetRootsList()
        {
            return Json(this.GetList(), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public FileResult GetRootsList1()
        {
            var stream = new MemoryStream();
            this.GetList().WriteTo(stream);
            stream.Position = 0L;

            return new FileStreamResult(stream, "application/json; charset=utf-8");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult GetARKeys()
        {
            return Json(this.GetListOfAR());
        }


        /// <summary>
        /// The get item.
        /// </summary>
        /// <param name="firstEventPulicKey">
        /// The first event pulic key.
        /// </param>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <returns>
        /// The <see cref="Stream"/>.
        /// </returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult GetItem(string firstEventPulicKey, string length)
        {
            var item = this.GetItemInt(firstEventPulicKey, length);
            if (item == null)
            {
                return null;
            }

            var outResult = Json(item, JsonRequestBehavior.AllowGet);
            return outResult;
        }

        private ImportSynchronizationMessage GetItemInt(string firstEventPulicKey, string length)
        {
            Guid syncProcess = Guid.NewGuid();

            Guid key;
            if (!Guid.TryParse(firstEventPulicKey, out key))
            {
                return null;
            }

            int ln;
            if (!int.TryParse(length, out ln))
            {
                return null;
            }

            var result = new ImportSynchronizationMessage();

            try
            {
                var process = (IEventSyncProcess)this.syncProcessFactory.GetProcess(SyncProcessType.Event, syncProcess, null);

                result = process.Export("Supervisor export AR events", key, ln);
            }
            catch (Exception ex)
            {
            }

            return result;
        }


        private ImportSynchronizationMessage GetARInt(string aRKey, string length, string rootType)
        {
            Guid syncProcess = Guid.NewGuid();

            Guid key;
            if (!Guid.TryParse(aRKey, out key))
            {
                return null;
            }

            int ln;
            if (!int.TryParse(length, out ln))
            {
                return null;
            }

            var result = new ImportSynchronizationMessage();

            try
            {
                var process = (IEventSyncProcess)this.syncProcessFactory.GetProcess(SyncProcessType.Event, syncProcess, null);

                result = process.GetAR("Supervisor export AR events", key,rootType, ln);
            }
            catch (Exception ex)
            {
            }

            return result;
        }


        /// <summary>
        /// The get item.
        /// </summary>
        /// <param name="firstEventPulicKey">
        /// The first event pulic key.
        /// </param>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <returns>
        /// The <see cref="Stream"/>.
        /// </returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public FileResult GetItemAsStream(string firstEventPulicKey, string length)
        {
            var item = this.GetItemInt(firstEventPulicKey, length);
            if (item == null)
            {
                return null;
            }

            var stream = new MemoryStream();
            item.WriteTo(stream);
            stream.Position = 0L;
            return new FileStreamResult(stream, "application/json; charset=utf-8");
        }


        /// <summary>
        /// Retrive Item
        /// </summary>
        /// <param name="aRKey"></param>
        /// <param name="length"></param>
        /// <param name="rootType"></param>
        /// <returns></returns>
        [CompressContent]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult GetAR(string aRKey, string length, string rootType)
        {
            var item = this.GetARInt(aRKey, length, rootType);
            
            if (item == null)
            {
                return null;
            }

            var stream = new MemoryStream();
            item.WriteTo(stream);
            stream.Position = 0L;
            
            return new FileStreamResult(stream, "application/octet-stream");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public bool PostStream(string request)
        {
            Guid syncProcess = Guid.NewGuid();

            try
            {
                Request.InputStream.Position = 0;

                /*var message = new EventSyncMessage();
                message.InitializeFrom(Request.InputStream);*/

                var settings = new JsonSerializerSettings();
                settings.TypeNameHandling = TypeNameHandling.Objects;

                string item;
                using (var sr = new StreamReader(Request.InputStream))
                {
                    item = sr.ReadToEnd();
                }
                
                EventSyncMessage message = JsonConvert.DeserializeObject<EventSyncMessage>(item, settings);

                if (message == null)
                {
                    return false;
                }

                var process =
                    (IEventSyncProcess)
                    this.syncProcessFactory.GetProcess(SyncProcessType.Event, syncProcess, message.SynchronizationKey);

                process.Import("Direct controller syncronization.", message.Command);

                return true;
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Fatal("Error on Sync.", ex);
                return false;
            }
        }

        #endregion
    }
}