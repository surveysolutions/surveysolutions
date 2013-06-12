// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImportExportController.cs" company="">
//   
// </copyright>
// <summary>
//   The import export controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Net;
using System.Web.Security;
using Main.Core.Entities.SubEntities;
using Main.Core.Events;
using Main.Core.View.User;
using WB.UI.Shared.Web.Exceptions;
using WB.UI.Shared.Web.Filters;
namespace Web.Supervisor.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Web;
    using System.Web.Mvc;

    using Core.Supervisor.Views.SyncProcess;

    using DataEntryClient.SycProcessFactory;

    using Main.Core.Export;
    using Main.Core.View;
    using Main.Synchronization.SyncManager;
    using Main.Synchronization.SyncSreamProvider;
    using Main.Synchronization.SyncStreamCollector;

    using SynchronizationMessages.Synchronization;


    using Newtonsoft.Json;

    using Questionnaire.Core.Web.Helpers;
    using Questionnaire.Core.Web.Threading;

    using SynchronizationMessages.CompleteQuestionnaire;

    using WB.Core.SharedKernel.Logger;

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

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILog logger;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly WB.Core.Synchronization.SyncManager.ISyncManager syncManager;

        #endregion

        #region Constructors and Destructors

        
        public ImportExportController(
            IDataExport exporter, IViewRepository viewRepository, ISyncProcessFactory syncProcessFactory, WB.Core.Synchronization.SyncManager.ISyncManager syncManager, ILog logger)
        {
            this.exporter = exporter;
            this.viewRepository = viewRepository;
            this.syncProcessFactory = syncProcessFactory;
            this.syncManager = syncManager;

            this.logger = logger;
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
                        var process = this.syncProcessFactory.GetUsbProcess(syncProcess);

                        this.AsyncManager.Parameters["result"] =
                            process.Export("Export DB on ViewerId in zip file");
                    }
                    catch (Exception e)
                    {
                        this.AsyncManager.Parameters["result"] = null;
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
        [Authorize(Roles = "Headquarter")]
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
                    var process = this.syncProcessFactory.GetUsbProcess( syncProcess);

                    process.Import(zipData, "Usb syncronization");
                }
                catch (Exception e)
                {
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

        private SyncItemsMetaContainer GetListOfAR(Guid supervisorId)
        {
            Guid syncProcess = Guid.NewGuid();

            var result = new SyncItemsMetaContainer();

            try
            {
                var process = this.syncProcessFactory.GetRestProcess(syncProcess, supervisorId);

                result = process.GetListOfAggregateRoots("ViewerId export AR events");
            }
            catch (Exception ex)
            {
                logger.Fatal("Error on retrieving the list of AR on sync. ", ex);
                logger.Fatal(ex.Message);
                logger.Fatal(ex.StackTrace);
            }

            return result;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [HandleUIException]
        public JsonResult GetARKeys(string login, string password)
        {
            var user = GetUser(login, password);
            if (user == null)
                throw new HttpStatusException(HttpStatusCode.Forbidden);

            return Json(this.GetListOfAR(user.Supervisor.Id));
        }

        private ImportSynchronizationMessage GetARInt(Guid supervisorId, string aRKey, string length, string rootType)
        {
            var result = new ImportSynchronizationMessage();

            Guid syncProcess = Guid.NewGuid();

            Guid key;
            if (!Guid.TryParse(aRKey, out key))
            {
                return result;
            }

            int ln;
            if (!int.TryParse(length, out ln))
            {
                return result;
            }
            
            try
            {
                var process = this.syncProcessFactory.GetRestProcess(syncProcess, supervisorId);
                result = process.GetAR("ViewerId export AR events.", key, rootType, ln);
            }
            catch (Exception ex)
            {
                logger.Fatal("Error on retrieving AR on sync. ", ex);
            }

            return result;
        }

        [CompressContent]
        [AcceptVerbs(HttpVerbs.Post)]
        [HandleUIException]
        public ActionResult GetAR(string aRKey, string length, string rootType, string login, string password)
        {
            var user = GetUser(login, password);
            if (user==null)
                throw new HttpStatusException(HttpStatusCode.Forbidden);

            var item = this.GetARInt(user.Supervisor.Id, aRKey, length, rootType);
            
            if (item == null)
            {
                return null;
            }

            var stream = new MemoryStream();
            item.WriteTo(stream);
            stream.Position = 0L;
            
            return new FileStreamResult(stream, "application/octet-stream");
        }


        protected UserView GetUser(string login, string password)
        {

            if (Membership.ValidateUser(login, password))
            {
                if (Roles.IsUserInRole(login, UserRoles.Operator.ToString()))
                {
                    return
                        this.viewRepository.Load<UserViewInputModel, UserView>(new UserViewInputModel(login, null));
                    
                }
            }
            return null;
        }
        
        [AcceptVerbs(HttpVerbs.Post)]
        [HandleUIException]
        public bool PostStream(string login, string password)
        {
            var user = GetUser(login, password);
            if (user == null)
                throw new HttpStatusException(HttpStatusCode.Forbidden);
            if (Request.Files==null || Request.Files.Count == 0)
                return false;
            try
            {
                EventSyncMessage message = null;

                string item = PackageHelper.Decompress(Request.Files[0].InputStream);
                
                try
                {
                    var settings = new JsonSerializerSettings();
                    settings.TypeNameHandling = TypeNameHandling.Objects;

                    message = JsonConvert.DeserializeObject<EventSyncMessage>(item, settings);
                }
                catch (Exception)
                {
                    logger.Fatal("Error on Deserialization received stream. Item: " + item);
                    throw;
                }
                
                if (message == null)
                {
                    return false;
                }

                 var process =
                    this.syncProcessFactory.GetRestProcess(message.SynchronizationKey, user.Supervisor.Id);


                process.Import("Direct controller syncronization.", message.Command);

                return true;
            }
            catch (Exception ex)
            {
                logger.Fatal("Error on Sync.", ex);
                logger.Fatal("Exception message: " + ex.Message);
                logger.Fatal("Stack: " + ex.StackTrace);
                
                return false;
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [HandleUIException]
        public bool Handshake(string login, string password, string clientID, string LastSyncID)
        {
            var user = GetUser(login, password);
            if (user == null)
                throw new HttpStatusException(HttpStatusCode.Forbidden);
            return true;
        }

        
        [AcceptVerbs(HttpVerbs.Post)]
        [HandleUIException]
        public ActionResult GetSyncPackage(string aRKey, string rootType, string login, string password)
        {
            var user = GetUser(login, password);
            if (user == null)
                throw new HttpStatusException(HttpStatusCode.Forbidden);

            Guid key;
            if (!Guid.TryParse(aRKey, out key))
            {
                return null; //todo: return correct description
            }

            try
            {
                var package = syncManager.ReceiveSyncPackage(null, key, rootType);
                return Json(package, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Fatal("Error on sync", ex);
                return null;
            }
        }

        #endregion
    }
}