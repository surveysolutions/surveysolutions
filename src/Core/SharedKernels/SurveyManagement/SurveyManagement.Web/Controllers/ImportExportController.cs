using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
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
        private readonly ILogger logger;

        public ImportExportController(
            ILogger logger, IBackupManager backupManager)
        {
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
                        this.logger.Error("Error on import ", e);
                    }
                };
            ThreadPool.QueueUserWorkItem(callback, syncProcess);

            return syncProcess;
        }

        public ActionResult ImportCompleted()
        {
            return this.RedirectToAction("Index", "Survey");
        }
    }
}