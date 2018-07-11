using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Properties;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation
{
    public class BrokenInterviewPackageSynchronizer : IBrokenInterviewPackageSynchronizer
    {
        private readonly IPlainStorage<BrokenInterviewPackageView, int?> brokenInterviewPackageStorage;
        private readonly ISupervisorSynchronizationService synchronizationService;

        public BrokenInterviewPackageSynchronizer(IPlainStorage<BrokenInterviewPackageView, int?> brokenInterviewPackageStorage,
            ISupervisorSynchronizationService synchronizationService)
        {
            this.brokenInterviewPackageStorage = brokenInterviewPackageStorage;
            this.synchronizationService = synchronizationService;
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
                    return;

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
        }
    }
}
