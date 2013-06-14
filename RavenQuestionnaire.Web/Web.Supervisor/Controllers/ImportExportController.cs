using System.Net;
using System.Web.Security;
using Main.Core.Entities.SubEntities;
using Main.Core.View.User;
using WB.Core.Synchronization.ImportManager;
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

    using Main.Core.Export;
    using Main.Core.View;

    using SynchronizationMessages.Synchronization;


    using Newtonsoft.Json;

    using Questionnaire.Core.Web.Helpers;
    using Questionnaire.Core.Web.Threading;

    using SynchronizationMessages.CompleteQuestionnaire;

    using WB.Core.SharedKernel.Logger;

    using Web.Supervisor.Models;
    using Web.Supervisor.Utils.Attributes;

    [NoAsyncTimeout]
    public class ImportExportController : AsyncController
    {
        private readonly IDataExport exporter;
        private readonly IImportManager importManager;
        private readonly ILog logger;
        private readonly WB.Core.Synchronization.SyncManager.ISyncManager syncManager;
        private readonly IViewFactory<SyncProcessLogInputModel, SyncProcessLogView> syncProcessLogViewFactory;
        private readonly IViewFactory<UserViewInputModel, UserView> userViewFactory;

        public ImportExportController(
            IImportManager importManager, WB.Core.Synchronization.SyncManager.ISyncManager syncManager,
            ILog logger, IViewFactory<SyncProcessLogInputModel, SyncProcessLogView> syncProcessLogViewFactory,
            IViewFactory<UserViewInputModel, UserView> userViewFactory)
        {
            this.exporter = exporter;
            this.importManager = importManager;
            this.syncManager = syncManager;
            this.logger = logger;
            this.userViewFactory = userViewFactory;
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
                    importManager.Import(zipData);
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
    }
}