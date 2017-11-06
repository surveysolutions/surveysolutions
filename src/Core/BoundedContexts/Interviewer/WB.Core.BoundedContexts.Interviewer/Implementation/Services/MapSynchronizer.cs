using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services.MapService;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class MapSyncProvider : IMapSyncProvider
    {
        private readonly ISynchronizationService synchronizationService;
        private readonly ILogger logger;
        private readonly IMapService mapService;

        public MapSyncProvider(ISynchronizationService synchronizationService,
            ILogger logger,
            IMapService mapService)
        {
            this.synchronizationService = synchronizationService;
            this.logger = logger;
            this.mapService = mapService;
        }

        public async Task SyncronizeMapsAsync(IProgress<MapSyncProgress> progress, CancellationToken cancellationToken)
        {
            try
            {
                progress.Report(new MapSyncProgress
                {
                    Title = $"Checking maps on server",
                    Status = MapSyncStatus.Started
                });


                var items = await this.synchronizationService.GetMapList(cancellationToken).ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();
                var processedMapsCount = 0;

                foreach (var mapDescription in items)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    processedMapsCount++;
                    progress.Report(new MapSyncProgress
                    {
                        Title = $"Handling map {mapDescription.MapName}, {processedMapsCount} out of {items.Count}",
                        Status = MapSyncStatus.Download
                    });


                    if (this.mapService.DoesMapExist(mapDescription.MapName))
                        continue;

                    void OnDownloadProgressChanged(DownloadProgressChangedEventArgs args)
                    {
                        if (args.ProgressPercentage % 5 == 0)
                        {
                            progress.Report(new MapSyncProgress
                            {
                                Title =
                                    $"Handling map {mapDescription}, {processedMapsCount} out of {items.Count}. \r\n Downloaded {args.BytesReceived} out of {args.TotalBytesToReceive} ({args.ProgressPercentage}%)",
                                Status = MapSyncStatus.Download
                            });
                        }
                    }

                    var mapContent = await this.synchronizationService
                        .GetMapContent(mapDescription.MapName, cancellationToken, OnDownloadProgressChanged)
                        .ConfigureAwait(false);

                    this.mapService.SaveMap(mapDescription.MapName, mapContent);
                }

                progress.Report(new MapSyncProgress
                {
                    Title = "Synchronization finished",
                    Status = MapSyncStatus.Success,
                });
            }
            catch (SynchronizationException ex)
            {
                var errorTitle = "Map sync error";
                var errorDescription = ex.Message;
                logger.Error("Map sync error", ex);

                switch (ex.Type)
                {
                    case SynchronizationExceptionType.RequestCanceledByUser:
                        progress.Report(new MapSyncProgress
                        {
                            Title = errorTitle,
                            Description = errorDescription,
                            Status = MapSyncStatus.Canceled
                        });
                        break;
                    case SynchronizationExceptionType.Unauthorized:
                    case SynchronizationExceptionType.UserLinkedToAnotherDevice:
                        progress.Report(new MapSyncProgress
                        {
                            Title = InterviewerUIResources.Synchronization_UserLinkedToAnotherDevice_Status,
                            Description = InterviewerUIResources.Synchronization_UserLinkedToAnotherDevice_Title,
                            Status = MapSyncStatus.Fail
                        });
                        break;
                    case SynchronizationExceptionType.UnacceptableSSLCertificate:
                        progress.Report(new MapSyncProgress
                        {
                            Title = InterviewerUIResources.UnexpectedException,
                            Description = InterviewerUIResources.UnacceptableSSLCertificate,
                            Status = MapSyncStatus.Fail
                        });
                        break;
                    default:
                        progress.Report(new MapSyncProgress
                        {
                            Title = errorTitle,
                            Description = errorDescription,
                            Status = MapSyncStatus.Fail
                        });
                        break;
                }
            }
            catch (Exception e)
            {
                logger.Error("Map sync error", e);
                progress.Report(new MapSyncProgress
                {
                    Status = MapSyncStatus.Fail,
                    Description = e.Message,
                    Title = "Map sync error"
                });
            }

        }

    }
}
