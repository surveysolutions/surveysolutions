using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using CAPI.Android.Core.Model;
using CAPI.Android.Core.Model.Authorization;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Implementation.Services.Rest;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Capi.Services;
using WB.UI.Capi.Settings;
using WB.UI.Capi.Utils;

namespace WB.UI.Capi.Syncronization
{
    public class SynchronozationProcessor
    {
        private readonly ILogger logger;
        private readonly ISynchronizationService synchronizationService;
        private readonly IInterviewerSettings interviewerSettings;
        private readonly Context context;
        private CancellationTokenSource cancellationSource;
        
        private readonly ICapiDataSynchronizationService dataProcessor;
        private readonly ICapiCleanUpService cleanUpExecutor;
        private readonly IInterviewSynchronizationFileStorage fileSyncRepository;
        private readonly ISyncPackageIdsStorage packageIdStorage;

        private readonly ISyncAuthenticator authentificator;
        private SyncCredentials credentials;
        
        public event EventHandler<SynchronizationEventArgs> StatusChanged;
        public event EventHandler ProcessFinished;
        public event EventHandler ProcessCanceling;
        public event EventHandler<SynchronizationCanceledEventArgs> ProcessCanceled;

        public SynchronozationProcessor(Context context, 
            ISyncAuthenticator authentificator, 
            ICapiDataSynchronizationService dataProcessor,
            ICapiCleanUpService cleanUpExecutor, 
            IInterviewSynchronizationFileStorage fileSyncRepository,
            ISyncPackageIdsStorage packageIdStorage,
            ILogger logger,
            ISynchronizationService synchronizationService,
            IInterviewerSettings interviewerSettings)
        {
            this.context = context;

            this.Preparation();
            this.authentificator = authentificator;
            this.cleanUpExecutor = cleanUpExecutor;
            this.fileSyncRepository = fileSyncRepository;
            this.packageIdStorage = packageIdStorage;
            this.logger = logger;
            this.synchronizationService = synchronizationService;
            this.interviewerSettings = interviewerSettings;
            this.dataProcessor = dataProcessor;
        }

        private async Task PullAsync()
        {
            this.ExitIfCanceled();
            this.OnStatusChanged(new SynchronizationEventArgsWithPercent("Pulling", Operation.Pull, true, 0));

            var cancellationToken = this.cancellationSource.Token;
            await this.MigrateOldSyncTimestampToId(cancellationToken);

            var remoteChuncksForDownload = new List<SynchronizationChunkMeta>();

            bool foundNeededPackages = false;
            var lastKnownPackageId = this.packageIdStorage.GetLastStoredChunkId();
            int returnedBackCount = 0;

            do
            {
                try
                {
                    this.OnStatusChanged(new SynchronizationEventArgsWithPercent("Receiving list of packageIdStorage to download", Operation.Pull, true, 0));

                    remoteChuncksForDownload = await this.synchronizationService.GetChunksAsync(credentials: credentials, token: cancellationToken);

                    foundNeededPackages = true;
                }
                catch (RestException ex)
                {
                    if (ex.StatusCode != 410) continue;

                    returnedBackCount++;
                    this.OnStatusChanged(new SynchronizationEventArgsWithPercent(
                        string.Format("Last known package not found on server. Searching for previous. Tried {0} packageIdStorage", returnedBackCount), 
                        Operation.Pull, true, 0));
                    lastKnownPackageId = this.packageIdStorage.GetChunkBeforeChunkWithId(lastKnownPackageId);
                }
            } while (!foundNeededPackages);
                
            int progressCounter = 0;
            foreach (SynchronizationChunkMeta chunk in remoteChuncksForDownload)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                try
                {
                    SyncItem data = await this.synchronizationService.RequestChunkAsync(
                        credentials: credentials, 
                        chunkId: chunk.Id,
                        token: cancellationToken);

                    this.dataProcessor.SavePulledItem(data);

                    this.packageIdStorage.Append(chunk.Id);
                }
                catch (Exception e)
                {
                    this.logger.Error(string.Format("Chunk {0} wasn't processed", chunk), e);
                    throw;
                }

                this.OnStatusChanged(new SynchronizationEventArgsWithPercent("pulling", Operation.Pull, true,
                    ((progressCounter++) * 100) / remoteChuncksForDownload.Count));
            }
        }

        private async Task MigrateOldSyncTimestampToId(CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(this.interviewerSettings.GetLastReceivedPackageId()))
            {
                long lastReceivedPackageIdOfLongType;
                if (!long.TryParse(this.interviewerSettings.GetLastReceivedPackageId(), out lastReceivedPackageIdOfLongType))
                    return;
                this.OnStatusChanged(new SynchronizationEventArgs("Tablet had old installation. Migrating pacakge timestamp to it's id", Operation.Pull, true));
                Guid lastReceivedChunkId = await this.synchronizationService.GetChunkIdByTimestamp(timestamp: lastReceivedPackageIdOfLongType, credentials: credentials, token: cancellationToken);
                this.packageIdStorage.Append(lastReceivedChunkId);
                this.interviewerSettings.SetLastReceivedPackageId(null);
            }
        }

        private async Task PushAsync()
        {
            this.ExitIfCanceled();
            this.OnStatusChanged(new SynchronizationEventArgsWithPercent("pushing", Operation.Push, true, 0));

            var dataByChuncks = this.dataProcessor.GetItemsForPush();
            int chunksCounter = 1;
            foreach (var chunckDescription in dataByChuncks)
            {
                this.ExitIfCanceled();

                await this.synchronizationService.PushChunkAsync(credentials : credentials, chunkAsString: chunckDescription.Content, token: this.cancellationSource.Token);

                this.fileSyncRepository.MoveInterviewsBinaryDataToSyncFolder(chunckDescription.EventSourceId);

                this.cleanUpExecutor.DeleteInterview(chunckDescription.EventSourceId);

                this.OnStatusChanged(new SynchronizationEventArgsWithPercent("pushing", Operation.Push, true, (chunksCounter * 100) / dataByChuncks.Count));
                chunksCounter++;
            }

            this.OnStatusChanged(new SynchronizationEventArgsWithPercent("pushing binary data", Operation.Push, true, 0));

            var binaryDatas = this.fileSyncRepository.GetBinaryFilesFromSyncFolder();
            int binaryDataCounter = 1;
            foreach (var binaryData in binaryDatas)
            {
                this.ExitIfCanceled();

                try
                {
                    await this.synchronizationService.PushBinaryAsync(
                        credentials: credentials,
                        fileData: binaryData.GetData(), 
                        fileName: binaryData.FileName,
                        interviewId: binaryData.InterviewId, 
                        token: this.cancellationSource.Token);

                    this.fileSyncRepository.RemoveBinaryDataFromSyncFolder(binaryData.InterviewId, binaryData.FileName);
                }
                catch (Exception e)
                {
                    this.logger.Error(e.Message, e);
                }

                this.OnStatusChanged(new SynchronizationEventArgsWithPercent("pushing binary data", Operation.Push, true, (binaryDataCounter * 100) / binaryDatas.Count));
                binaryDataCounter++;
            }
        }

        private async void Handshake()
        {
            this.ExitIfCanceled();

            var userCredentials = this.authentificator.RequestCredentials();

            this.ExitIfCanceled();

            if (!userCredentials.HasValue)
                throw new AuthenticationException("User wasn't authenticated.");

            this.credentials = userCredentials.Value;

            this.OnStatusChanged(
                new SynchronizationEventArgs("Connecting...", Operation.Handshake, true));

            var clientRegistrationId = await this.synchronizationService.HandshakeAsync(credentials : credentials);

            this.interviewerSettings.SetClientRegistrationId(clientRegistrationId);
        }

        public void Run()
        {
            this.cancellationSource = new CancellationTokenSource();
            Task.Run(() => this.RunInternalAsync());
        }

        private async Task RunInternalAsync()
        {
            try
            {
                this.Handshake();
                await this.PushAsync();
                await this.PullAsync();

                this.OnProcessFinished();
            }
            catch (Exception e)
            {
               this.OnProcessCanceled(new List<Exception>(){e});
            }
        }

        public void Cancel(Exception exception = null)
        {
            this.OnStatusChanged(new SynchronizationEventArgs("Cancelling", Operation.Pull, false));
            this.cancellationSource.Cancel();
        }

        protected void OnProcessFinished()
        {
            if (this.cancellationSource.IsCancellationRequested)
                return;
            var handler = this.ProcessFinished;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        protected void OnProcessCanceling()
        {
            var handler = this.ProcessCanceling;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        protected void OnProcessCanceled(IList<Exception> exceptions)
        {
            var handler = this.ProcessCanceled;
            if (handler != null)
                handler(this, new SynchronizationCanceledEventArgs(exceptions));
        }
        protected void OnStatusChanged(SynchronizationEventArgs evt)
        {
            if (this.cancellationSource.IsCancellationRequested)
                return;
            var handler = this.StatusChanged;
            if (handler != null)
                handler(this, evt);
        }

        protected void Preparation()
        {
            if (!NetworkHelper.IsNetworkEnabled(this.context))
            {
                throw new InvalidOperationException("Network is not available.");
            }
        }

        private void ExitIfCanceled()
        {
            var cancellationToken1 = this.cancellationSource.Token;
            if (cancellationToken1.IsCancellationRequested)
                cancellationToken1.ThrowIfCancellationRequested();
        }
    }
}