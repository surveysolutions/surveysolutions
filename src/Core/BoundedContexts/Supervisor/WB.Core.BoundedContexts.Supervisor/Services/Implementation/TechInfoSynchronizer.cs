using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Properties;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation
{
    public class TechInfoSynchronizer : ITechInfoSynchronizer
    {
        private readonly IPlainStorage<BrokenInterviewPackageView, int?> brokenInterviewPackageStorage;
        private readonly ISupervisorSynchronizationService synchronizationService;
        private readonly IPlainStorage<UnexpectedExceptionFromInterviewerView, int?> unexpectedExceptionsStorage;
        private readonly ITabletInfoService tabletInfoService;

        public TechInfoSynchronizer(IPlainStorage<BrokenInterviewPackageView, int?> brokenInterviewPackageStorage,
            ISupervisorSynchronizationService synchronizationService, 
            IPlainStorage<UnexpectedExceptionFromInterviewerView, int?> unexpectedExceptionsStorage,
            ITabletInfoService tabletInfoService)
        {
            this.brokenInterviewPackageStorage = brokenInterviewPackageStorage;
            this.synchronizationService = synchronizationService;
            this.unexpectedExceptionsStorage = unexpectedExceptionsStorage;
            this.tabletInfoService = tabletInfoService;
        }

        public async Task SynchronizeAsync(IProgress<SyncProgressInfo> progress, SynchronizationStatistics statistics, CancellationToken cancellationToken)
        {
            progress.Report(new SyncProgressInfo
            {
                Title = SupervisorUIResources.Synchronization_UploadBrokenInterviewPackages
            });

            while (true)
            {
                var brokenInterviewPackage = brokenInterviewPackageStorage.FirstOrDefault();
                if (brokenInterviewPackage == null)
                    break;

                var brokenInterviewPackageApiView = new BrokenInterviewPackageApiView()
                {
                    InterviewId = brokenInterviewPackage.InterviewId,
                    InterviewKey = brokenInterviewPackage.InterviewKey,
                    QuestionnaireId = brokenInterviewPackage.QuestionnaireId,
                    QuestionnaireVersion = brokenInterviewPackage.QuestionnaireVersion,
                    ResponsibleId = brokenInterviewPackage.ResponsibleId,
                    InterviewStatus = brokenInterviewPackage.InterviewStatus,
                    Events = brokenInterviewPackage.Events,
                    IncomingDate = brokenInterviewPackage.IncomingDate,
                    ProcessingDate = brokenInterviewPackage.ProcessingDate,
                    ExceptionType = brokenInterviewPackage.ExceptionType,
                    ExceptionMessage = brokenInterviewPackage.ExceptionMessage,
                    ExceptionStackTrace = brokenInterviewPackage.ExceptionStackTrace,
                    PackageSize = brokenInterviewPackage.PackageSize,
                };

                await this.synchronizationService.UploadBrokenInterviewPackageAsync(brokenInterviewPackageApiView, cancellationToken);

                brokenInterviewPackageStorage.Remove(brokenInterviewPackage.Id);
            };

            progress.Report(new SyncProgressInfo
            {
                Title = SupervisorUIResources.Synchronization_UploadExceptions
            });

            var exceptions = this.unexpectedExceptionsStorage.LoadAll().ToList();
            await this.synchronizationService.UploadInterviewerExceptionsAsync(exceptions, cancellationToken);
            this.unexpectedExceptionsStorage.RemoveAll();

            while (true)
            {
                var tabletInfo = tabletInfoService.GetTopRecordForSync();
                if (tabletInfo == null)
                    break;
                
                await this.synchronizationService.UploadTabletInfoAsync(tabletInfo.DeviceInfo, cancellationToken);

                tabletInfoService.Remove(tabletInfo.Id);
            };
        }
    }
}
