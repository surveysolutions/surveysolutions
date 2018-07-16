using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.MapService;
using WB.Core.SharedKernels.Enumerator.Services.MapSynchronization;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.MapSynchronization
{
    public abstract class MapSyncProviderBase : AbstractSynchronizationProcess, IMapSyncProvider
    {
        private readonly ISynchronizationService synchronizationService;
        private readonly ILogger logger;
        private readonly IMapService mapService;

        protected MapSyncProviderBase(IMapService mapService, ISynchronizationService synchronizationService, ILogger logger,
            IHttpStatistician httpStatistician, IUserInteractionService userInteractionService, IPrincipal principal,
            IPasswordHasher passwordHasher, 
            IPlainStorage<InterviewView> interviewViewRepository, IAuditLogService auditLogService) 
            : base(synchronizationService, logger,
            httpStatistician, userInteractionService, principal, passwordHasher, 
            interviewViewRepository, auditLogService)
        {
            this.synchronizationService = synchronizationService;
            this.logger = logger;
            this.mapService = mapService;
        }

        protected override bool SendStatistics => false;
        protected override string SucsessDescription => InterviewerUIResources.Maps_Synchronization_Success_Description;

        public override async Task Synchronize(IProgress<SyncProgressInfo> progress, CancellationToken cancellationToken, SynchronizationStatistics statistics)
        {
            progress.Report(new SyncProgressInfo
            {
                Title = InterviewerUIResources.MapSyncProvider_SyncronizeMapsAsync_Checking_maps_on_server,
                Status = SynchronizationStatus.Started
            });

            var items = await this.synchronizationService.GetMapList(cancellationToken).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();
            var processedMapsCount = 0;

            foreach (var mapDescription in items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                processedMapsCount++;

                if (this.mapService.DoesMapExist(mapDescription.MapName))
                    continue;

                void OnDownloadProgressChanged(TransferProgress args)
                {
                    if (args.ProgressPercentage % 5 == 0)
                    {
                        progress.Report(new SyncProgressInfo
                        {
                            Title =
                                string.Format(InterviewerUIResources.MapSyncProvider_SyncronizeMapsAsync_Progress_Report_Format,
                                                mapDescription.MapName, processedMapsCount, items.Count, args.ProgressPercentage),
                            Status = SynchronizationStatus.Download
                        });
                    }
                }

                try
                {
                    long downloded = 0;
                    using (var streamToSave = this.mapService.GetTempMapSaveStream(mapDescription.MapName))
                    using (var contentStreamResult = await this.synchronizationService
                        .GetMapContentStream(mapDescription.MapName, cancellationToken).ConfigureAwait(false))
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                        }
                        
                        var buffer = new byte[1024];
                        var downloadProgressChangedEventArgs = new TransferProgress()
                        {
                            TotalBytesToReceive = contentStreamResult.ContentLength
                        };

                        int read;
                        while ((read = await contentStreamResult.Stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)
                                   .ConfigureAwait(false)) > 0)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                            }

                            downloded += read;

                            streamToSave.Write(buffer, 0, read);

                            if (contentStreamResult.ContentLength != null)
                                downloadProgressChangedEventArgs.ProgressPercentage =
                                    Math.Min(Math.Round((decimal)(100 * downloded) / contentStreamResult.ContentLength.Value), 100);

                            downloadProgressChangedEventArgs.BytesReceived = downloded;
                            OnDownloadProgressChanged(downloadProgressChangedEventArgs);
                        }
                    }
                    
                    this.mapService.MoveTempMapToPermanent(mapDescription.MapName);
                }
                catch (Exception ex)
                {
                    logger.Error("Error on Map sync", ex);
                    throw;
                }
            }

            progress.Report(new SyncProgressInfo
            {
                Title = InterviewerUIResources.MapSyncProvider_SyncronizeMapsAsync_Map_synchronization_succesfuly_finished,
                Status = SynchronizationStatus.Success,
            });
        }

        public override SyncStatisticsApiView ToSyncStatisticsApiView(SynchronizationStatistics statistics, Stopwatch stopwatch)
        {
            return new SyncStatisticsApiView();
        }

        protected override void CheckAfterStartSynchronization(CancellationToken cancellationToken)
        {
            
        }
    }
}
