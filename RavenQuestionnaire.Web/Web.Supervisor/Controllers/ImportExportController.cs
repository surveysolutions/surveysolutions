using System;
using System.Collections.Generic;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Main.Core.Export;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Threading;
using SynchronizationMessages.Export;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Synchronization;

namespace Web.Supervisor.Controllers
{
    [NoAsyncTimeout]
    public class ImportExportController : AsyncController
    {
        private readonly IBackupManager backupManager;
        private readonly IDataExportService exporter;
        private readonly ILogger logger;

        public ImportExportController(
            ILogger logger, IDataExportService exporter, IBackupManager backupManager)
        {
            this.exporter = exporter;
            this.logger = logger;
            this.backupManager = backupManager;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Import()
        {
            return this.View("ViewTestUploadFile");
        }

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
                        this.backupManager.Restore(zipData);
                    }
                    catch (Exception e)
                    {
                        this.logger.Fatal("Error on import ", e);
                    }
                };
            ThreadPool.QueueUserWorkItem(callback, syncProcess);

            return syncProcess;
        }

        public ActionResult ImportCompleted()
        {
            return this.RedirectToAction("Index", "Survey");
        }

        [Authorize(Roles = "Headquarter")]
        public void GetExportedDataAsync(Guid id,long version, string type)
        {
            if ((id == null) || (id == Guid.Empty) || string.IsNullOrEmpty(type))
            {
                throw new HttpException(404, "Invalid query string parameters");
            }

            AsyncQuestionnaireUpdater.Update(
                this.AsyncManager,
                () =>
                    {
                        try
                        {
                            this.AsyncManager.Parameters["result"] = this.exporter.ExportData(id, version, type);
                        }
                        catch
                        {
                            this.AsyncManager.Parameters["result"] = null;
                        }
                    });
        }

        public ActionResult GetExportedDataCompleted(Dictionary<string, byte[]> result)
        {
            return this.File(ZipFileData.ExportInternal(result, string.Format("exported{0}.zip", DateTime.Now.ToLongTimeString())), "application/zip", "data.zip");
        }


        public void BackupAsync(Guid syncKey)
        {
            AsyncQuestionnaireUpdater.Update(
                this.AsyncManager,
                () =>
                    {
                        try
                        {
                            ZipFileData data = this.backupManager.Backup();
                            this.AsyncManager.Parameters["result"] = ZipHelper.Compress(data);
                        }
                        catch (Exception e)
                        {
                            this.AsyncManager.Parameters["result"] = null;
                            this.logger.Fatal("Error on export " + e.Message, e);
                            if (e.InnerException != null)
                            {
                                this.logger.Fatal("Error on export (Inner Exception)", e.InnerException);
                            }
                        }
                    });
        }

        public ActionResult BackupCompleted(byte[] result)
        {
            return this.File(result, "application/zip",
                             string.Format("backup_{0}.zip", DateTime.UtcNow.ToString("yyyyMMddhhnnss")));
        }
    }
}