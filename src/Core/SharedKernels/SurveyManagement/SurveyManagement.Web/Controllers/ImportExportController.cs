using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.Storage;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Compression;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Threading;
using WB.Core.Synchronization;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [NoAsyncTimeout]
    public class ImportExportController : AsyncController
    {
        private readonly IBackupManager backupManager;
        private readonly IFilebasedExportedDataAccessor exportDataAccessor;
        private readonly ILogger logger;

        public ImportExportController(
            ILogger logger, IFilebasedExportedDataAccessor exportDataAccessor, IBackupManager backupManager)
        {
            this.exportDataAccessor = exportDataAccessor;
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

        [Authorize(Roles = "Administrator, Headquarter")]
        public void GetExportedDataAsync(Guid id, long version)
        {
            if (id == Guid.Empty)
            {
                throw new HttpException(404, "Invalid query string parameters");
            }
            AsyncQuestionnaireUpdater.Update(
                this.AsyncManager,
                () =>
                {
                    IsolatedThreadManager.MarkCurrentThreadAsIsolated();
                    try
                    {
                        this.AsyncManager.Parameters["result"] =
                            this.exportDataAccessor.GetFilePathToExportedCompressedData(id, version);
                    }
                    catch (Exception exc)
                    {
                        this.logger.Error("Error occurred during export. " + exc.Message, exc);
                        this.AsyncManager.Parameters["result"] = null;
                    }
                    finally
                    {
                        IsolatedThreadManager.ReleaseCurrentThreadFromIsolation();
                    }
                });
        }

        public ActionResult GetExportedDataCompleted(string result)
        {
            return this.File(result, "application/zip", fileDownloadName: Path.GetFileName(result));
        }

        [Authorize(Roles = "Administrator, Headquarter")]
        public void GetExportedApprovedDataAsync(Guid id, long version)
        {
            if (id == Guid.Empty)
            {
                throw new HttpException(404, "Invalid query string parameters");
            }
            AsyncQuestionnaireUpdater.Update(
                this.AsyncManager,
                () =>
                {
                    IsolatedThreadManager.MarkCurrentThreadAsIsolated();
                    try
                    {
                        this.AsyncManager.Parameters["result"] =
                            this.exportDataAccessor.GetFilePathToExportedApprovedCompressedData(id, version);
                    }
                    catch (Exception exc)
                    {
                        this.logger.Error("Error occurred during export. " + exc.Message, exc);
                        this.AsyncManager.Parameters["result"] = null;
                    }
                    finally
                    {
                        IsolatedThreadManager.ReleaseCurrentThreadFromIsolation();
                    }
                });
        }

        public ActionResult GetExportedApprovedDataCompleted(string result)
        {
            return this.File(result, "application/zip", fileDownloadName: Path.GetFileName(result));
        }

        [Authorize(Roles = "Administrator, Headquarter")]
        public void GetExportedFilesAsync(Guid id, long version)
        {
            AsyncQuestionnaireUpdater.Update(
                this.AsyncManager,
                () =>
                {
                    try
                    {
                        this.AsyncManager.Parameters["result"] = this.exportDataAccessor.GetFilePathToExportedBinaryData(id, version);
                    }
                    catch (Exception exc)
                    {
                        this.logger.Error("Error occurred during export. " + exc.Message, exc);
                        this.AsyncManager.Parameters["result"] = null;
                    }
                });
        }

        public ActionResult GetExportedFilesCompleted(string result)
        {
            return this.File(result, "application/zip", fileDownloadName: Path.GetFileName(result));
        }

        public void GetExportedHistoyAsync(Guid id, long version)
        {
            AsyncQuestionnaireUpdater.Update(
                this.AsyncManager,
                () =>
                {
                    try
                    {
                        this.AsyncManager.Parameters["result"] = this.exportDataAccessor.GetFilePathToExportedCompressedHistoryData(id, version);
                    }
                    catch (Exception exc)
                    {
                        this.logger.Error("Error occurred during export. " + exc.Message, exc);
                        this.AsyncManager.Parameters["result"] = null;
                    }
                });
        }

        public ActionResult GetExportedHistoyCompleted(string result)
        {
            return this.File(result, "application/zip", fileDownloadName: Path.GetFileName(result));
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