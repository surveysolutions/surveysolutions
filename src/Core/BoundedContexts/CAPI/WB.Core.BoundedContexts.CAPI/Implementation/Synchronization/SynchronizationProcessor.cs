using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using WB.Core.BoundedContexts.Capi.Implementation.Authorization;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Capi.Implementation.Synchronization
{
    public class SynchronizationProcessor
    {
        private readonly ILogger logger;
        private readonly ISynchronizationService synchronizationService;
        private readonly IInterviewerSettings interviewerSettings;
        private CancellationToken cancellationToken;
        private CancellationTokenSource cancellationTokenSource;
        
        private readonly ICapiDataSynchronizationService dataProcessor;
        private readonly ICapiCleanUpService cleanUpExecutor;
        private readonly IInterviewSynchronizationFileStorage fileSyncRepository;
        private readonly ISyncPackageIdsStorage packageIdStorage;

        private readonly IDeviceChangingVerifier deviceChangingVerifier;

        private readonly ISyncAuthenticator authentificator;
        private SyncCredentials credentials;

        private Guid userId = Guid.Empty;

        public event EventHandler<SynchronizationEventArgs> StatusChanged;
        public event System.EventHandler ProcessFinished;
        public event System.EventHandler ProcessCanceling;
        public event EventHandler<SynchronizationCanceledEventArgs> ProcessCanceled;

        private bool wasSynchronizationSuccessfull = false;
        public bool WasSynchronizationSuccessfull
        {
            get
            {
                return this.wasSynchronizationSuccessfull;
            }
        }

        public SynchronizationProcessor(
            IDeviceChangingVerifier deviceChangingVerifier,
            ISyncAuthenticator authentificator, 
            ICapiDataSynchronizationService dataProcessor,
            ICapiCleanUpService cleanUpExecutor, 
            IInterviewSynchronizationFileStorage fileSyncRepository,
            ISyncPackageIdsStorage packageIdStorage,
            ILogger logger,
            ISynchronizationService synchronizationService,
            IInterviewerSettings interviewerSettings)
        {
            this.deviceChangingVerifier = deviceChangingVerifier;
            this.authentificator = authentificator;
            this.cleanUpExecutor = cleanUpExecutor;
            this.fileSyncRepository = fileSyncRepository;
            this.packageIdStorage = packageIdStorage;
            this.logger = logger;
            this.synchronizationService = synchronizationService;
            this.interviewerSettings = interviewerSettings;
            this.dataProcessor = dataProcessor;
        }

        public Task Run()
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            this.cancellationToken = this.cancellationTokenSource.Token;

            return Task.Run(async () =>
            {
                try
                {
                    await this.Handshake();
                    await this.Push();
                    await this.Pull();

                    this.wasSynchronizationSuccessfull = true;

                    this.OnProcessFinished();
                }
                catch (Exception e)
                {
                    this.OnProcessCanceled(e);
                }

            }, this.cancellationToken);
        }

        private async Task Handshake()
        {
            this.ExitIfCanceled();

            var userCredentials = this.authentificator.RequestCredentials();

            this.ExitIfCanceled();

            if (!userCredentials.HasValue)
                throw new Exception("User wasn't authenticated.");

            this.credentials = userCredentials.Value;

            this.OnStatusChanged(new SynchronizationEventArgs("Connecting...", Operation.Handshake, true));

            var isThisExpectedDevice = await this.synchronizationService.CheckExpectedDeviceAsync(credentials: this.credentials);

            var shouldThisDeviceBeLinkedToUser = false;
            if (!isThisExpectedDevice)
            {
                shouldThisDeviceBeLinkedToUser = this.deviceChangingVerifier.ConfirmDeviceChanging();
            }

            this.ExitIfCanceled();

            HandshakePackage package = await this.synchronizationService.HandshakeAsync(credentials: this.credentials, shouldThisDeviceBeLinkedToUser: shouldThisDeviceBeLinkedToUser);

            this.userId = package.UserId;

            this.interviewerSettings.SetClientRegistrationId(package.ClientRegistrationKey);

            if (shouldThisDeviceBeLinkedToUser)
            {
                this.cleanUpExecutor.DeleteAllInterviewsForUser(this.userId);
            }
        }

        private async Task Pull()
        {
            this.ExitIfCanceled();
            this.OnStatusChanged(new SynchronizationEventArgsWithPercent("Pulling", Operation.Pull, true, 0));

            await this.PullUserPackages();
            await this.PullQuestionnairePackages();
            await this.PullInterviewPackages();
        }

        private async Task Push()
        {
            this.ExitIfCanceled();
            this.OnStatusChanged(new SynchronizationEventArgsWithPercent("Pushing interview data", Operation.Push, true, 0));

            var dataByChuncks = this.dataProcessor.GetItemsToPush(userId);
            int chunksCounter = 1;
            foreach (var chunckDescription in dataByChuncks)
            {
                this.ExitIfCanceled();

                await this.synchronizationService.PushChunkAsync(credentials: this.credentials, chunkAsString: chunckDescription.Content, interviewId: chunckDescription.EventSourceId);

                this.fileSyncRepository.MoveInterviewsBinaryDataToSyncFolder(chunckDescription.EventSourceId);

                this.cleanUpExecutor.DeleteInterview(chunckDescription.EventSourceId);

                this.OnStatusChanged(new SynchronizationEventArgsWithPercent(string.Format("Pushing chunk {0} out of {1}", chunksCounter, dataByChuncks.Count), Operation.Push, true, (chunksCounter * 100) / dataByChuncks.Count));
                chunksCounter++;
            }

            this.OnStatusChanged(new SynchronizationEventArgsWithPercent("Pushing binary data", Operation.Push, true, 0));

            var binaryDatas = this.fileSyncRepository.GetBinaryFilesFromSyncFolder();
            int binaryDataCounter = 1;
            foreach (var binaryData in binaryDatas)
            {
                this.ExitIfCanceled();

                try
                {
                    await this.synchronizationService.PushBinaryAsync(
                        credentials: this.credentials,
                        interviewId: binaryData.InterviewId,
                        fileName: binaryData.FileName,
                        fileData: binaryData.GetData());

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

        private async Task PullUserPackages()
        {
            var packageProcessor = new Func<SynchronizationChunkMeta, Task>(this.DownloadAndProcessUserPackage);
            await this.PullPackages(this.userId, "User", packageProcessor, SyncItemType.User);
        }

        private async Task PullQuestionnairePackages()
        {
            var packageProcessor = new Func<SynchronizationChunkMeta, Task>(this.DownloadAndProcessQuestionnirePackage);
            await this.PullPackages(Guid.Empty, "Questionnaire", packageProcessor, SyncItemType.Questionnaire);
        }

        private async Task PullInterviewPackages()
        {
            var packageProcessor = new Func<SynchronizationChunkMeta, Task>(this.DownloadAndProcessInterviewPackage);
            await this.PullPackages(this.userId, "Interview", packageProcessor, SyncItemType.Interview);
        }

        private async Task PullPackages(Guid currentUserId, string type, Func<SynchronizationChunkMeta, Task> packageProcessor, string packageType)
        {
            this.ExitIfCanceled();
            this.OnStatusChanged(new SynchronizationEventArgsWithPercent(
                string.Format("Pulling packages for {0}", type.ToLower()), Operation.Pull, true, 0));

            var syncItemsMetaContainer = await this.GetSyncItemsMetaContainer(currentUserId, type, packageType);

            int progressCounter = 0;
            int chunksToDownload = syncItemsMetaContainer.SyncPackagesMeta.Count();

            foreach (SynchronizationChunkMeta chunk in syncItemsMetaContainer.SyncPackagesMeta)
            {
                ExitIfCanceled();

                try
                {
                    await packageProcessor(chunk);
                }
                catch (Exception e)
                {
                    this.logger.Error(string.Format("{0} packge {1} wasn't processed", type, chunk.Id), e);
                    throw;
                }

                this.OnStatusChanged(
                    new SynchronizationEventArgsWithPercent(
                        string.Format("Pulling packages for {0}", type.ToLower()),
                        Operation.Pull,
                        true,
                        ((progressCounter++) * 100) / chunksToDownload));
            }
        }

        private async Task<SyncItemsMetaContainer> GetSyncItemsMetaContainer(Guid currentUserId, string type, string packageType)
        {
            SyncItemsMetaContainer syncItemsMetaContainer = null;

            bool foundNeededPackages = false;
            int returnedBackCount = 0;

            var lastKnownPackageId = this.packageIdStorage.GetLastStoredPackageId(packageType, currentUserId);
            do
            {
                this.OnStatusChanged(
                    new SynchronizationEventArgsWithPercent(
                        string.Format("Receiving list of packageIds for {0} to download", type.ToLower()),
                        Operation.Pull,
                        true,
                        0));

                syncItemsMetaContainer =
                    await this.synchronizationService.GetPackageIdsToDownloadAsync(this.credentials, type, lastKnownPackageId);

                if (syncItemsMetaContainer == null)
                {
                    returnedBackCount++;
                    this.OnStatusChanged(
                        new SynchronizationEventArgsWithPercent(
                            string.Format(
                                "Last known package for {0} not found on server. Searching for previous. Tried {1} packageIdStorage",
                                type.ToLower(),
                                returnedBackCount),
                            Operation.Pull,
                            true,
                            0));

                    lastKnownPackageId = this.packageIdStorage.GetChunkBeforeChunkWithId(lastKnownPackageId,
                        currentUserId);
                    continue;
                }
                foundNeededPackages = true;
            }
            while (!foundNeededPackages);
            return syncItemsMetaContainer;
        }

        private async Task DownloadAndProcessUserPackage(SynchronizationChunkMeta chunk)
        {
            var package = await this.synchronizationService.RequestUserPackageAsync(credentials: this.credentials, chunkId: chunk.Id);
            this.dataProcessor.ProcessDownloadedPackage(package);
            this.packageIdStorage.Append(package.PackageId, SyncItemType.User, this.userId, chunk.SortIndex);
        }

        private async Task DownloadAndProcessQuestionnirePackage(SynchronizationChunkMeta chunk)
        {
            var package = await this.synchronizationService.RequestQuestionnairePackageAsync(this.credentials, chunk.Id);
            this.dataProcessor.ProcessDownloadedPackage(package, chunk.ItemType);
            this.packageIdStorage.Append(package.PackageId, SyncItemType.Questionnaire, Guid.Empty, chunk.SortIndex);
        }

        private async Task DownloadAndProcessInterviewPackage(SynchronizationChunkMeta chunk)
        {
            var package = await this.synchronizationService.RequestInterviewPackageAsync(this.credentials, chunk.Id);
            this.dataProcessor.ProcessDownloadedPackage(package, chunk.ItemType, this.userId);
            this.packageIdStorage.Append(package.PackageId, SyncItemType.Interview, this.userId, chunk.SortIndex);
        }

        public void Cancel(Exception exception = null)
        {
            this.OnStatusChanged(new SynchronizationEventArgs("Cancelling", Operation.Pull, false));
            this.cancellationTokenSource.Cancel();
        }

        protected void OnProcessFinished()
        {
            if (this.cancellationToken.IsCancellationRequested)
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
        
        protected void OnProcessCanceled(Exception exception)
        {
            var handler = this.ProcessCanceled;
            if (handler != null)
                handler(this, new SynchronizationCanceledEventArgs(exception));
        }

        protected void OnStatusChanged(SynchronizationEventArgs evt)
        {
            if (this.cancellationToken.IsCancellationRequested)
                return;
            var handler = this.StatusChanged;
            if (handler != null)
                handler(this, evt);
        }

        private void ExitIfCanceled()
        {
            var cancellationToken1 = this.cancellationTokenSource.Token;
            if (cancellationToken1.IsCancellationRequested)
                cancellationToken1.ThrowIfCancellationRequested();
        }
    }
}