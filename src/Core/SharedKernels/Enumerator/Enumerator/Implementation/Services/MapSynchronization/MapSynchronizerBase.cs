using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.HttpServices.Services;
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
using Xamarin.Essentials;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.MapSynchronization
{
    public abstract class MapSyncProviderBase : AbstractSynchronizationProcess, IMapSyncProvider
    {
        private readonly ISynchronizationService synchronizationService;
        private readonly ILogger logger;
        private readonly IPermissionsService permissionsService;
        private readonly IMapService mapService;

        protected MapSyncProviderBase(IMapService mapService,
            ISynchronizationService synchronizationService,
            ILogger logger,
            IHttpStatistician httpStatistician,
            IPrincipal principal,
            IPlainStorage<InterviewView> interviewViewRepository,
            IAuditLogService auditLogService,
            IEnumeratorSettings enumeratorSettings,
            IUserInteractionService userInteractionService,
            IDeviceInformationService deviceInformationService,
            IServiceLocator serviceLocator,
            IAssignmentDocumentsStorage assignmentsStorage,
            IPermissionsService permissionsService)
            : base(synchronizationService, logger, httpStatistician, principal, interviewViewRepository,
                auditLogService, enumeratorSettings, serviceLocator, deviceInformationService, userInteractionService, assignmentsStorage)
        {
            this.synchronizationService = synchronizationService;
            this.logger = logger;
            this.permissionsService = permissionsService;
            this.mapService = mapService;
        }

        protected override bool SendStatistics => false;
        protected override string SuccessDescription => EnumeratorUIResources.Maps_Synchronization_Success_Description;

        public override async Task Synchronize(IProgress<SyncProgressInfo> progress, 
            CancellationToken cancellationToken, SynchronizationStatistics statistics)
        {
            progress.Report(new SyncProgressInfo
            {
                Title = EnumeratorUIResources.MapSyncProvider_SyncronizeMapsAsync_Checking_maps_on_server,
                Status = SynchronizationStatus.Started
            });
            
            await this.permissionsService.AssureHasManageExternalStoragePermission().ConfigureAwait(false);

            var items = await this.synchronizationService.GetMapList(cancellationToken).ConfigureAwait(false);
            var availableMapFiles = this.mapService.GetAvailableMaps(false);
            var availableShapefiles = this.mapService.GetAvailableShapefiles();
            var mapFileNames = availableMapFiles.Select(m => m.MapFileName)
                .Concat(availableShapefiles.Select(s => s.ShapefileFileName));

            foreach (var mapFileName in mapFileNames)
            {
                if (items.Exists(x => string.Compare(x.MapName, mapFileName, StringComparison.InvariantCultureIgnoreCase) == 0))
                    continue;

                try
                {
                    this.mapService.RemoveMap(mapFileName);
                }
                catch (Exception ex)
                {
                    logger.Info("Unable delete map", ex);
                }
            }

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
                                string.Format(EnumeratorUIResources.MapSyncProvider_SyncronizeMapsAsync_Progress_Report_Format,
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
                        .GetMapContentStream(mapDescription.MapName, cancellationToken)
                        .ConfigureAwait(false))
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
                Title = EnumeratorUIResources.MapSyncProvider_SyncronizeMapsAsync_Map_synchronization_succesfuly_finished,
                Status = SynchronizationStatus.Success,
            });
        }

        public override SyncStatisticsApiView ToSyncStatisticsApiView(SynchronizationStatistics statistics, Stopwatch stopwatch)
        {
            return new SyncStatisticsApiView();
        }

        protected override Task CheckAfterStartSynchronization(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        
        protected override Task RefreshUserInfo(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        
        protected override Task ChangeWorkspaceAndNavigateToItAsync()
            => throw new NotImplementedException("Remove workspace by offline synchronization no supported");
    }
}
