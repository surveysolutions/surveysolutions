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
using CAPI.Android.Syncronization.Handshake;
using CAPI.Android.Syncronization.Pull;
using CAPI.Android.Syncronization.Push;
using CAPI.Android.Syncronization.RestUtils;
using CAPI.Android.Utils;
using Microsoft.Practices.ServiceLocation;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace CAPI.Android.Syncronization
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

        private readonly PullDataProcessor pullDataProcessor;
        private readonly PushDataProcessor pushDataProcessor;

        private IDictionary<SynchronizationChunkMeta, bool> remoteChuncksForDownload;

        private readonly ISyncAuthenticator authentificator;
        private SyncCredentials credentials;

        private string clientRegistrationId 
        {
            set { SettingsManager.SetSetting(SettingsNames.RegistrationKeyName, value);}
            get { return SettingsManager.GetSetting(SettingsNames.RegistrationKeyName);}
        }

        private string lastSequence
        {
            set { SettingsManager.SetSetting(SettingsNames.LastHandledSequence, value); }
            get { return SettingsManager.GetSetting(SettingsNames.LastHandledSequence); }
        }

        public SynchronozationProcessor(Context context, ISyncAuthenticator authentificator, IChangeLogManipulator changelog, IReadSideRepositoryReader<LoginDTO> userStorage)
        {
            this.context = context;
            
            Preparation();
            this.authentificator = authentificator;

            var executor = new AndroidRestUrils(SettingsManager.GetSyncAddressPoint());
            pull = new RestPull(executor);
            push = new RestPush(executor);
            handshake = new RestHandshake(executor);

            var commandService = NcqrsEnvironment.Get<ICommandService>();
            pullDataProcessor = new PullDataProcessor(changelog, commandService, userStorage);
            pushDataProcessor = new PushDataProcessor(changelog);

            this.logger = ServiceLocator.Current.GetInstance<ILogger>();
        }

        #region operations
        
        private void Pull()
        {
            ExitIfCanceled();
            OnStatusChanged(new SynchronizationEventArgsWithPercent("pulling", Operation.Pull, true, 0));

            CancelIfException(() =>
                {
                    remoteChuncksForDownload = pull.GetChuncks(credentials.Login, credentials.Password, clientRegistrationId, lastSequence, ct);
                    
                    int progressCounter = 0;
                    foreach (var chunckId in remoteChuncksForDownload.Keys.ToList())
                    {
                        if (ct.IsCancellationRequested)
                            return;

                        try
                        {
                            var data = pull.RequestChunck(credentials.Login, credentials.Password, chunckId.Id, chunckId.Sequence, clientRegistrationId, ct);

                            pullDataProcessor.Save(data);
                            remoteChuncksForDownload[chunckId] = true;

                            pullDataProcessor.Proccess(chunckId);
                            //save last handled item
                            lastSequence = chunckId.Sequence.ToString();
                        }
                        catch (Exception e)
                        {
                            logger.Error(string.Format("chunk {0} wasn't processed", chunckId), e);
                            
                            throw;
                        }

                        OnStatusChanged(new SynchronizationEventArgsWithPercent("pulling", Operation.Pull, true,
                                                                                ((progressCounter++) * 100) / remoteChuncksForDownload.Count));
                    }
                });
        }

        private void Push()
        {

            ExitIfCanceled();
            OnStatusChanged(new SynchronizationEventArgsWithPercent("pushing", Operation.Push, true, 0));

            CancelIfException(() =>
                {
                    var dataByChuncks = pushDataProcessor.GetChuncks();
                    int i = 1;
                    foreach (var chunckDescription in dataByChuncks)
                    {
                        ExitIfCanceled();

                        push.PushChunck(credentials.Login, credentials.Password, chunckDescription.Content, ct);
                        //fix method
                        pushDataProcessor.DeleteInterview(chunckDescription.EventSourceId);

                        OnStatusChanged(new SynchronizationEventArgsWithPercent("pushing", Operation.Push, true, (i * 100) / dataByChuncks.Count));
                        i++;
                    }
                });
        }

        private void Handshake()
        {
            ExitIfCanceled();

            CancelIfException(() =>
                {
                    var androidId = SettingsManager.AndroidId;
                    var appId = SettingsManager.InstallationId;
                    var userCredentials = authentificator.RequestCredentials();
                    ExitIfCanceled();

                    if (!userCredentials.HasValue)
                        throw new AuthenticationException("User wasn't authenticated.");
                    credentials = userCredentials.Value;


                    //string message = string.Format("handshake app {0}, device {1}", appId, androidId);
                    string message = "connecting...";
                    OnStatusChanged(
                        new SynchronizationEventArgs(message, Operation.Handshake, true));
                    var registrationKey = SettingsManager.GetSetting(SettingsNames.RegistrationKeyName);
                    clientRegistrationId = handshake.Execute(credentials.Login, credentials.Password, androidId, appId, clientRegistrationId);
                });
        }

        #endregion

        public void Run()
        {
            tokenSource2 = new CancellationTokenSource();
            ct = tokenSource2.Token;
            task = Task.Factory.StartNew(RunInternal, ct);
        }

        private void RunInternal()
        {
            Handshake();
            Push();
            Pull();
            

            OnProcessFinished();
        }

        public void Cancel()
        {
            if(tokenSource2.IsCancellationRequested)
                return;
            Task.Factory.StartNew(CancelInternal);
        }

        private void CancelInternal()
        {
            OnProcessCanceling();

            tokenSource2.Cancel();
           
            List<Exception> exceptions = new List<Exception>();
            try
            {
                Task.WaitAll(task);
            }
            catch (AggregateException e)
            {
                exceptions = e.InnerExceptions.ToList();
            }
            exceptions.Add(new Exception("Synchronization wasn't completed"));
            OnProcessCanceled(exceptions);
        }

        #region events

        public event EventHandler<SynchronizationEventArgs> StatusChanged;
        public event EventHandler ProcessFinished;
        public event EventHandler ProcessCanceling;
        public event EventHandler<SynchronizationCanceledEventArgs> ProcessCanceled;

        protected void OnProcessFinished()
        {
            if(tokenSource2.IsCancellationRequested)
                return;
            var handler = ProcessFinished;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        protected void OnProcessCanceling()
        {
            var handler = ProcessCanceling;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        protected void OnProcessCanceled(IList<Exception> exceptions)
        {
            var handler = ProcessCanceled;
            if (handler != null)
                handler(this, new SynchronizationCanceledEventArgs(exceptions));
        }
        protected void OnStatusChanged(SynchronizationEventArgs evt)
        {
            if(tokenSource2.IsCancellationRequested)
                return;
            var handler = StatusChanged;
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

            if (!NetworkHelper.IsNetworkEnabled(context))
            {
                
                throw new InvalidOperationException("Network is not avalable.");
                return;
            }
        }

        private void ExitIfCanceled()
        {
            if (ct.IsCancellationRequested)
                ct.ThrowIfCancellationRequested();
        }

        private void CancelIfException(Action action)
        {
            try
            {
                action();
            }
            catch (Exception exc)
            {
                logger.Error("Error occured during the process. Pcocess is being canceled.", exc);
                Cancel();
                throw;
            }
        }

    }
}