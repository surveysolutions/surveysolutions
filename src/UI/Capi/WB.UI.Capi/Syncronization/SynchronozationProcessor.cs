using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using CAPI.Android.Core.Model;
using CAPI.Android.Core.Model.Authorization;
using CAPI.Android.Core.Model.ViewModel.Login;
using CAPI.Android.Settings;
using Main.Core.View;
using Microsoft.Practices.ServiceLocation;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi.Synchronization;
using WB.Core.BoundedContexts.Capi.Synchronization.ChangeLog;
using WB.Core.BoundedContexts.Capi.Synchronization.Services;
using WB.Core.BoundedContexts.Capi.Synchronization.Views.Login;
using WB.Core.GenericSubdomains.Rest;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Utils.Compression;
using WB.Core.SharedKernel.Utils.Serialization;
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
        private CancellationToken ct;
        private CancellationTokenSource tokenSource2;
        private Task task;

        #region rest comunicators

        private readonly RestPull pull;
        private readonly RestHandshake handshake;
        private readonly RestPush push;

        #endregion

        private readonly ICapiDataSynchronizationService dataProcessor;
        private readonly ICapiCleanUpService cleanUpExecutor;
        private readonly IInterviewSynchronizationFileStorage fileSyncRepository;
        private IDictionary<SynchronizationChunkMeta, bool> remoteChuncksForDownload;

        private readonly ISyncAuthenticator authentificator;
        private SyncCredentials credentials;

        private string clientRegistrationId 
        {
            set { SettingsManager.SetSetting(SettingsNames.RegistrationKeyName, value);}
            get { return SettingsManager.GetSetting(SettingsNames.RegistrationKeyName);}
        }

        private string lastTimestamp
        {
            set { SettingsManager.SetSetting(SettingsNames.LastTimestamp, value); }
            get { return SettingsManager.GetSetting(SettingsNames.LastTimestamp); }
        }

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

        #region operations
        
        private void Pull()
        {
            this.ExitIfCanceled();
            this.OnStatusChanged(new SynchronizationEventArgsWithPercent("pulling", Operation.Pull, true, 0));

            this.CancelIfException(() =>
                {
                    this.remoteChuncksForDownload = this.pull.GetChuncks(this.credentials.Login, this.credentials.Password, this.clientRegistrationId, this.lastTimestamp, this.ct);
                    
                    int progressCounter = 0;
                    foreach (var chunckId in this.remoteChuncksForDownload.Keys.ToList())
                    {
                        if (this.ct.IsCancellationRequested)
                            return;

                        try
                        {
                            var data = this.pull.RequestChunck(this.credentials.Login, this.credentials.Password, chunckId.Id, chunckId.Timestamp, this.clientRegistrationId, this.ct);

                            this.dataProcessor.SavePulledItem(data);
                            this.remoteChuncksForDownload[chunckId] = true;
                            
                            //save last handled item
                            this.lastTimestamp = chunckId.Timestamp.ToString();
                        }
                        catch (Exception e)
                        {
                            this.logger.Error(string.Format("chunk {0} wasn't processed", chunckId), e);
                            
                            throw;
                        }

                        this.OnStatusChanged(new SynchronizationEventArgsWithPercent("pulling", Operation.Pull, true,
                                                                                ((progressCounter++) * 100) / this.remoteChuncksForDownload.Count));
                    }
                });
        }

        private void Push()
        {

            this.ExitIfCanceled();
            this.OnStatusChanged(new SynchronizationEventArgsWithPercent("pushing", Operation.Push, true, 0));

            this.CancelIfException(() =>
                {
                    var dataByChuncks = this.dataProcessor.GetItemsForPush();
                    int i = 1;
                    foreach (var chunckDescription in dataByChuncks)
                    {
                        this.ExitIfCanceled();

                        this.push.PushChunck(this.credentials.Login, this.credentials.Password, chunckDescription.Content, this.ct);

                        this.fileSyncRepository.MoveInterviewsBinaryDataToSyncFolder(chunckDescription.EventSourceId);
                        //fix method
                        cleanUpExecutor.DeleteInterview(chunckDescription.EventSourceId);

                        this.OnStatusChanged(new SynchronizationEventArgsWithPercent("pushing", Operation.Push, true, (i * 100) / dataByChuncks.Count));
                        i++;
                    }
                });

            this.OnStatusChanged(new SynchronizationEventArgsWithPercent("pushing binary data", Operation.Push, true, 0));
            this.CancelIfException(() =>
            {
                var binaryDatas = this.fileSyncRepository.GetBinaryFilesFromSyncFolder();
                int i = 1;
                foreach (var binaryData in binaryDatas)
                {
                    this.ExitIfCanceled();

                    try
                    {
                        this.push.PushBinary(this.credentials.Login, this.credentials.Password, binaryData.GetData(), binaryData.FileName,
                            binaryData.InterviewId, this.ct);

                        this.fileSyncRepository.RemoveBinaryDataFromSyncFolder(binaryData.InterviewId, binaryData.FileName);
                    }
                    catch (Exception e)
                    {
                        logger.Error(e.Message, e);
                    }

                    this.OnStatusChanged(new SynchronizationEventArgsWithPercent("pushing binary data", Operation.Push, true, (i * 100) / binaryDatas.Count));
                    i++;
                }
            });
        }

        private void Handshake()
        {
            this.ExitIfCanceled();

            this.CancelIfException(() =>
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
                    var registrationKey = SettingsManager.GetSetting(SettingsNames.RegistrationKeyName);
                    this.clientRegistrationId = this.handshake.Execute(this.credentials.Login, this.credentials.Password, androidId, appId, this.clientRegistrationId);
                });
        }

        #endregion

        public void Run()
        {
            this.tokenSource2 = new CancellationTokenSource();
            this.ct = this.tokenSource2.Token;
            this.task = Task.Factory.StartNew(this.RunInternal, this.ct);
        }

        private void RunInternal()
        {
            this.Handshake();
            this.Push();
            this.Pull();
            

            this.OnProcessFinished();
        }

        public void Cancel()
        {
            if(this.tokenSource2.IsCancellationRequested)
                return;
            Task.Factory.StartNew(this.CancelInternal);
        }

        private void CancelInternal()
        {
            this.OnProcessCanceling();

            this.tokenSource2.Cancel();
           
            List<Exception> exceptions = new List<Exception>();
            try
            {
                Task.WaitAll(this.task);
            }
            catch (AggregateException e)
            {
                exceptions = e.InnerExceptions.ToList();
            }
            exceptions.Add(new Exception("Synchronization wasn't completed"));
            this.OnProcessCanceled(exceptions);
        }

        #region events

        public event EventHandler<SynchronizationEventArgs> StatusChanged;
        public event EventHandler ProcessFinished;
        public event EventHandler ProcessCanceling;
        public event EventHandler<SynchronizationCanceledEventArgs> ProcessCanceled;

        protected void OnProcessFinished()
        {
            if(this.tokenSource2.IsCancellationRequested)
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
            if(this.tokenSource2.IsCancellationRequested)
                return;
            var handler = this.StatusChanged;
            if (handler != null)
                handler(this, evt);
        }
        #endregion

 
        protected void Preparation()
        {
            
            if (!SettingsManager.CheckSyncPoint())
            {
                
                throw new InvalidOperationException("Sync point is set incorrect.");
                return;
            }

            if (!NetworkHelper.IsNetworkEnabled(this.context))
            {
                
                throw new InvalidOperationException("Network is not available.");
                return;
            }
        }

        private void ExitIfCanceled()
        {
            if (this.ct.IsCancellationRequested)
                this.ct.ThrowIfCancellationRequested();
        }

        private void CancelIfException(Action action)
        {
            try
            {
                action();
            }
            catch (Exception exc)
            {
                this.logger.Error("Error occurred during the process. Process is being canceled.", exc);
                this.Cancel();
                throw;
            }
        }

    }
}