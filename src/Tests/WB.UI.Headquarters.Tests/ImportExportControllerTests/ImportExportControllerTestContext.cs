using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Moq;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.Synchronization;
using WB.UI.Headquarters.Controllers;

namespace WB.UI.Headquarters.Tests.ImportExportControllerTests
{
    internal class ImportExportControllerTestContext
    {
        protected static ImportExportController CreateImportExportController(IFilebaseExportDataAccessor dataExportRepositoryWriter = null)
        {
            return new ImportExportController(Mock.Of<ILogger>(), dataExportRepositoryWriter ?? Mock.Of<IFilebaseExportDataAccessor>(), Mock.Of<IBackupManager>());
        }

        public static void ExecuteAsync(AsyncController asyncController,
                                       Action actionAsync,
                                       Action actionCompleted)
        {
            var trigger = new AutoResetEvent(false);
            asyncController.AsyncManager.Finished += (sender, ev) =>
            {
                actionCompleted();
                trigger.Set();
            };
            actionAsync();
            trigger.WaitOne();
        }
    }
}
