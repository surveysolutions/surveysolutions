using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using CAPI.Android.Core.Model;
using CAPI.Android.Core.Model.Authorization;
using CAPI.Android.Settings;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils.Rest;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Capi.Settings;
using WB.UI.Capi.Syncronization.Handshake;
using WB.UI.Capi.Syncronization.Pull;
using WB.UI.Capi.Syncronization.Push;
using WB.UI.Capi.Utils;

namespace WB.UI.Capi.Syncronization
{
    public class SynchronozationProcessor
    {
        private readonly ILogger logger;
        private readonly Context context;
        private CancellationTokenSource cancellationSource;
        
        private readonly RestPull pull;
        private readonly RestHandshake handshake;
        private readonly RestPush push;

        private readonly ICapiDataSynchronizationService dataProcessor;
        private readonly ICapiCleanUpService cleanUpExecutor;
        private readonly IInterviewSynchronizationFileStorage fileSyncRepository;
        private IDictionary<SynchronizationChunkMeta, bool> remoteChuncksForDownload;

        private readonly ISyncAuthenticator authentificator;
        private SyncCredentials credentials;
        private Task synchronizationTask;

        private string clientRegistrationId
        {
            set { SettingsManager.SetSetting(SettingsNames.RegistrationKeyName, value); }
            get { return SettingsManager.GetSetting(SettingsNames.RegistrationKeyName); }
        }

        private string lastTimestamp
        {
            set { SettingsManager.SetSetting(SettingsNames.LastTimestamp, value); }
            get { return SettingsManager.GetSetting(SettingsNames.LastTimestamp); }
        }

        public event EventHandler<SynchronizationEventArgs> StatusChanged;
        public event EventHandler ProcessFinished;
        public event EventHandler ProcessCanceling;
        public event EventHandler<SynchronizationCanceledEventArgs> ProcessCanceled;

        public SynchronozationProcessor(Context context, ISyncAuthenticator authentificator, ICapiDataSynchronizationService dataProcessor,
            ICapiCleanUpService cleanUpExecutor, IRestServiceWrapperFactory restServiceWrapperFactory, IInterviewSynchronizationFileStorage fileSyncRepository)
        {
            this.context = context;

            this.Preparation();
            this.authentificator = authentificator;
            this.cleanUpExecutor = cleanUpExecutor;
            this.fileSyncRepository = fileSyncRepository;

            var executor = restServiceWrapperFactory.CreateRestServiceWrapper(SettingsManager.GetSyncAddressPoint());
            this.pull = new RestPull(executor);
            this.push = new RestPush(executor);
            this.handshake = new RestHandshake(executor);

            this.dataProcessor = dataProcessor;

            this.logger = ServiceLocator.Current.GetInstance<ILogger>();
        }

        private async Task PullAsync()
        {
            this.ExitIfCanceled();
            this.OnStatusChanged(new SynchronizationEventArgsWithPercent("pulling", Operation.Pull, true, 0));

            try
            {
                var cancellationToken = this.cancellationSource.Token;
                this.remoteChuncksForDownload = await this.pull.GetChuncksAsync(this.credentials.Login, 
                    this.credentials.Password, 
                    this.clientRegistrationId, 
                    this.lastTimestamp, 
                    cancellationToken);

                int progressCounter = 0;
                foreach (SynchronizationChunkMeta chunckId in this.remoteChuncksForDownload.Keys.ToList())
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    try
                    {
                        SyncItem data = await this.pull.RequestChunckAsync(this.credentials.Login, 
                            this.credentials.Password, 
                            chunckId.Id, 
                            this.clientRegistrationId, 
                            cancellationToken);

                        this.dataProcessor.SavePulledItem(data);
                        this.remoteChuncksForDownload[chunckId] = true;

                        // save last handled item
                        this.lastTimestamp = chunckId.Id.ToString();
                    }
                    catch (Exception e)
                    {
                        this.logger.Error(string.Format("chunk {0} wasn't processed", chunckId), e);
                        throw;
                    }

                    this.OnStatusChanged(new SynchronizationEventArgsWithPercent("pulling", Operation.Pull, true,
                        ((progressCounter++) * 100) / this.remoteChuncksForDownload.Count));
                }
            }
            catch (Exception e)
            {
                this.logger.Error("Error occurred during pull process. Process is being canceled.", e);
                this.Cancel();
                throw;
            }
        }

        private async Task PushAsync()
        {
            this.ExitIfCanceled();
            this.OnStatusChanged(new SynchronizationEventArgsWithPercent("pushing", Operation.Push, true, 0));

            try
            {
                var dataByChuncks = this.dataProcessor.GetItemsForPush();
                int chunksCounter = 1;
                foreach (var chunckDescription in dataByChuncks)
                {
                    this.ExitIfCanceled();

                    await this.push.PushChunck(this.credentials.Login, this.credentials.Password, chunckDescription.Content, this.cancellationSource.Token);

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
                        await this.push.PushBinary(this.credentials.Login, this.credentials.Password, binaryData.GetData(), binaryData.FileName,
                            binaryData.InterviewId, this.cancellationSource.Token);

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
            catch (Exception e)
            {
                this.logger.Error("Error occurred during push process. Process is being canceled.", e);
                this.Cancel();
                throw;
            }
        }

        private void Handshake()
        {
            this.ExitIfCanceled();

            try
            {
                var androidId = SettingsManager.AndroidId;
                var appId = SettingsManager.InstallationId;
                var userCredentials = this.authentificator.RequestCredentials();
                this.ExitIfCanceled();

                if (!userCredentials.HasValue)
                    throw new AuthenticationException("User wasn't authenticated.");
                this.credentials = userCredentials.Value;


                //string message = string.Format("handshake app {0}, device {1}", appId, androidId);
                string message = "connecting...";
                this.OnStatusChanged(
                    new SynchronizationEventArgs(message, Operation.Handshake, true));
                
                this.clientRegistrationId = this.handshake.Execute(this.credentials.Login, this.credentials.Password, androidId, appId, this.clientRegistrationId);
            }
            catch (Exception e)
            {
                this.logger.Error("Error occurred during handshake process. Process is being canceled.", e);
                this.Cancel();
                throw;
            }
        }

        public void Run()
        {
            this.cancellationSource = new CancellationTokenSource();
            this.synchronizationTask = Task.Run(() => this.RunInternalAsync());
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
                this.OnProcessCanceled(new List<Exception> {e});
                throw;
            }
        }

        public void Cancel()
        {
            if (this.cancellationSource.IsCancellationRequested)
                return;
            Task.Factory.StartNew(this.CancelInternal);
        }

        private void CancelInternal()
        {
            this.OnProcessCanceling();

            var exceptions = new List<Exception>();

            try
            {
                this.cancellationSource.Cancel();
                this.synchronizationTask.Wait(this.cancellationSource.Token);
            }
            catch (AggregateException e)
            {
                exceptions = e.InnerExceptions.ToList();
            }
            exceptions.Add(new Exception("Synchronization wasn't completed"));
            this.OnProcessCanceled(exceptions);
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
            if (!SettingsManager.CheckSyncPoint())
            {
                throw new InvalidOperationException("Sync point is set incorrect.");
            }

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