using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using WB.Core.GenericSubdomains.Portable.Services;
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
        public void GetAllDataAsync(Guid id, long version, ExportDataType type = ExportDataType.Tab)
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
                        this.AsyncManager.Parameters["result"] = this.exportDataAccessor.GetFilePathToExportedCompressedData(id, version, type);
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

        public ActionResult GetAllDataCompleted(string result)
        {
            return this.File(result, "application/zip", fileDownloadName: Path.GetFileName(result));
        }

        [Authorize(Roles = "Administrator, Headquarter")]
        public void GetApprovedDataAsync(Guid id, long version, ExportDataType type = ExportDataType.Tab)
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
                        this.AsyncManager.Parameters["result"] = this.exportDataAccessor.GetFilePathToExportedApprovedCompressedData(id, version, type);
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

        public ActionResult GetApprovedDataCompleted(string result)
        {
            return this.File(result, "application/zip", fileDownloadName: Path.GetFileName(result));
        }


        [Authorize(Roles = "Administrator, Headquarter")]
        public void GetFilesAsync(Guid id, long version)
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

        public ActionResult GetFilesCompleted(string result)
        {
            return this.File(result, "application/zip", fileDownloadName: Path.GetFileName(result));
        }

        public void GetHistoryAsync(Guid id, long version)
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

        public ActionResult GetHistoryCompleted(string result)
        {
            return this.File(result, "application/zip", fileDownloadName: Path.GetFileName(result));
        }
    }
}