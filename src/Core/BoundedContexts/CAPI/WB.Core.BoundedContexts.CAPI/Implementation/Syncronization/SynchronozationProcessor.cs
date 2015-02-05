using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Capi.Implementation.Authorization;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Capi.Implementation.Syncronization
{
    public class SynchronozationProcessor
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

        private readonly IDataCollectionAuthentication membership;

        public event EventHandler<SynchronizationEventArgs> StatusChanged;
        public event System.EventHandler ProcessFinished;
        public event System.EventHandler ProcessCanceling;
        public event EventHandler<SynchronizationCanceledEventArgs> ProcessCanceled;

        public SynchronozationProcessor(
            IDeviceChangingVerifier deviceChangingVerifier,
            ISyncAuthenticator authentificator, 
            ICapiDataSynchronizationService dataProcessor,
            ICapiCleanUpService cleanUpExecutor, 
            IInterviewSynchronizationFileStorage fileSyncRepository,
            ISyncPackageIdsStorage packageIdStorage,
            ILogger logger,
            ISynchronizationService synchronizationService,
            IInterviewerSettings interviewerSettings,
            IDataCollectionAuthentication membership)
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
            this.membership = membership;
        }

        private async Task Pull()
        {
            this.ExitIfCanceled();
            this.OnStatusChanged(new SynchronizationEventArgsWithPercent("Pulling", Operation.Pull, true, 0));

            await this.MigrateOldSyncTimestampToId();

            SyncItemsMetaContainer syncItemsMetaContainer = null;

            bool foundNeededPackages = false;
            var lastKnownPackageId = this.packageIdStorage.GetLastStoredChunkId();
            //int returnedBackCount = 0;

            do
            {
                this.OnStatusChanged(new SynchronizationEventArgsWithPercent("Receiving list of packageIds to download", Operation.Pull, true, 0));

                syncItemsMetaContainer = await this.synchronizationService.GetChunksAsync(
                    credentials: this.credentials, 
                    lastKnownPackageId: lastKnownPackageId);

                //if (remoteChuncksForDownload == null)
                //{
                //    returnedBackCount++;
                //    this.OnStatusChanged(new SynchronizationEventArgsWithPercent(
                //        string.Format("Last known package not found on server. Searching for previous. Tried {0} packageIdStorage", returnedBackCount),
                //        Operation.Pull, true, 0));
                //    lastKnownPackageId = this.packageIdStorage.GetChunkBeforeChunkWithId(lastKnownPackageId);
                //    continue;
                //}

                foundNeededPackages = true;
            } while (!foundNeededPackages);
                
            int progressCounter = 0;
            int chunksToDownload = syncItemsMetaContainer.InterviewChunksMeta.Count()
                                   + syncItemsMetaContainer.QuestionnaireChunksMeta.Count()
                                   + syncItemsMetaContainer.UserChunksMeta.Count();

            foreach (SynchronizationChunkMeta chunk in syncItemsMetaContainer.UserChunksMeta)
            {
                if (this.cancellationToken.IsCancellationRequested)
                    return;

                try
                {
                    var data = await this.synchronizationService.RequestUserPackageAsync(credentials: this.credentials, chunkId: chunk.Id);

                    this.dataProcessor.ProcessDownloadedPackage((UserSyncPackageDto)data);

                    this.packageIdStorage.Append(chunk.Id);
                }
                catch (Exception e)
                {
                    this.logger.Error(string.Format("User packge {0} wasn't processed", chunk.Id), e);
                    throw;
                }

                this.OnStatusChanged(new SynchronizationEventArgsWithPercent("pulling", Operation.Pull, true,
                    ((progressCounter++) * 100) / chunksToDownload));
            }

            foreach (SynchronizationChunkMeta chunk in syncItemsMetaContainer.QuestionnaireChunksMeta)
            {
                if (this.cancellationToken.IsCancellationRequested)
                    return;

                try
                {
                    var data = await this.synchronizationService.RequestQuestionnairePackageAsync(credentials: this.credentials, chunkId: chunk.Id);

                    this.dataProcessor.ProcessDownloadedPackage((QuestionnaireSyncPackageDto)data);

                    this.packageIdStorage.Append(chunk.Id);
                }
                catch (Exception e)
                {
                    this.logger.Error(string.Format("Questionnaire packge  {0} wasn't processed", chunk.Id), e);
                    throw;
                }

                this.OnStatusChanged(new SynchronizationEventArgsWithPercent("pulling", Operation.Pull, true,
                    ((progressCounter++) * 100) / chunksToDownload));
            }

            foreach (SynchronizationChunkMeta chunk in syncItemsMetaContainer.InterviewChunksMeta)
            {
                if (this.cancellationToken.IsCancellationRequested)
                    return;

                try
                {
                    var data = await this.synchronizationService.RequestInterviewPackageAsync(credentials: this.credentials, chunkId: chunk.Id);

                    this.dataProcessor.ProcessDownloadedPackage((InterviewSyncPackageDto)data);

                    this.packageIdStorage.Append(chunk.Id);
                }
                catch (Exception e)
                {
                    this.logger.Error(string.Format("Interview packge  {0} wasn't processed", chunk.Id), e);
                    throw;
                }

                this.OnStatusChanged(new SynchronizationEventArgsWithPercent("pulling", Operation.Pull, true,
                    ((progressCounter++) * 100) / chunksToDownload));
            }
        }

        private async Task MigrateOldSyncTimestampToId()
        {
            string lastReceivedPackageId = this.interviewerSettings.GetLastReceivedPackageId();
            

            if (!string.IsNullOrEmpty(lastReceivedPackageId))
            {
                this.logger.Warn(string.Format("Migration of old version of sync. Last received package id: {0}", lastReceivedPackageId));

                long lastReceivedPackageIdOfLongType;
                if (!long.TryParse(lastReceivedPackageId, out lastReceivedPackageIdOfLongType))
                    return;
                this.OnStatusChanged(new SynchronizationEventArgs("Tablet had old installation. Migrating package timestamp to it's id", Operation.Pull, true));
                string lastReceivedChunkId = await this.synchronizationService.GetChunkIdByTimestampAsync(timestamp: lastReceivedPackageIdOfLongType, credentials: this.credentials);
                
                this.packageIdStorage.Append(lastReceivedChunkId);
                
                this.interviewerSettings.SetLastReceivedPackageId(null);
            }
        }

        private async Task Push()
        {
            this.ExitIfCanceled();
            this.OnStatusChanged(new SynchronizationEventArgsWithPercent("Pushing interview data", Operation.Push, true, 0));

            var dataByChuncks = this.dataProcessor.GetItemsForPush();
            int chunksCounter = 1;
            foreach (var chunckDescription in dataByChuncks)
            {
                this.ExitIfCanceled();

                await this.synchronizationService.PushChunkAsync(credentials : this.credentials, chunkAsString: chunckDescription.Content);

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
                        fileData: binaryData.GetData(), 
                        fileName: binaryData.FileName,
                        interviewId: binaryData.InterviewId);

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

        private async Task Handshake()
        {
            this.ExitIfCanceled();

            var userCredentials = this.authentificator.RequestCredentials();

            this.ExitIfCanceled();

            if (!userCredentials.HasValue)
                throw new Exception("User wasn't authenticated.");

            this.credentials = userCredentials.Value;
            
            this.OnStatusChanged(new SynchronizationEventArgs("Connecting...", Operation.Handshake, true));

            try
            {
                var isThisExpectedDevice = await this.synchronizationService.CheckExpectedDeviceAsync(credentials: this.credentials);

                var shouldThisDeviceBeLinkedToUser = false;
                if (!isThisExpectedDevice)
                {
                    shouldThisDeviceBeLinkedToUser = this.deviceChangingVerifier.ConfirmDeviceChanging();
                }

                Guid clientRegistrationId = await this.synchronizationService.HandshakeAsync(credentials: this.credentials, shouldThisDeviceBeLinkedToUser: shouldThisDeviceBeLinkedToUser);

                this.interviewerSettings.SetClientRegistrationId(clientRegistrationId);

                if (shouldThisDeviceBeLinkedToUser)
                {
                    var userId =  this.membership.GetUserIdByLoginIfExists(this.credentials.Login).Result;
                    if (userId.HasValue)
                    {
                        this.cleanUpExecutor.DeleteAllInterviewsForUser(userId.Value);
                    }
                }
            }
            catch (Exception e)
            {
                var knownHttpStatusCodes = new[] { HttpStatusCode.NotAcceptable, HttpStatusCode.InternalServerError, HttpStatusCode.Unauthorized };
                var restException = e as RestException;
                if (restException != null && !knownHttpStatusCodes.Contains(restException.StatusCode)) 
                    throw new RestException(string.Empty, restException.StatusCode, e);
                throw;
            }
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

                    this.OnProcessFinished();
                }
                catch (Exception e)
                {
                    this.OnProcessCanceled(new List<Exception>() {e});
                }

            }, this.cancellationToken);
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
        protected void OnProcessCanceled(IList<Exception> exceptions)
        {
            var handler = this.ProcessCanceled;
            if (handler != null)
                handler(this, new SynchronizationCanceledEventArgs(exceptions));
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