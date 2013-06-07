using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core;
using CAPI.Android.Core.Model.ChangeLog;
using CAPI.Android.Settings;
using CAPI.Android.Syncronization.Handshake;
using CAPI.Android.Syncronization.Pull;
using CAPI.Android.Syncronization.Push;
using CAPI.Android.Utils;
using Main.Synchronization.Credentials;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;

namespace CAPI.Android.Syncronization
{
    public class SynchronozationProcessor
    {
        
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
        
        private IDictionary<Guid, bool> remoteChuncksForDownload;

        private readonly ISyncAuthenticator authentificator;
        private SyncCredentials credentials;
        private Guid syncId;

        public SynchronozationProcessor(Context context, ISyncAuthenticator authentificator, IChangeLogManipulator changelog)
        {
            this.context = context;
            
            Preparation();
            this.authentificator = authentificator;

            pull = new RestPull(SettingsManager.GetSyncAddressPoint());
            push = new RestPush(SettingsManager.GetSyncAddressPoint());
            handshake = new RestHandshake(SettingsManager.GetSyncAddressPoint());

            pullDataProcessor = new PullDataProcessor(changelog, NcqrsEnvironment.Get<ICommandService>());
            pushDataProcessor = new PushDataProcessor(changelog);
        }

        #region operations

        private void Validate()
        {
            OnStatusChanged(new SynchronizationEventArgs("validating"));

            CancelIfException(() =>
                {
                    foreach (var chunck in remoteChuncksForDownload.Where(c => c.Value).Select(c => c.Key).ToList())
                    {
                        pullDataProcessor.Proccess(chunck);
                    }
                });
        }

        private void Pull()
        {
            ExitIfCanceled();
            OnStatusChanged(new SynchronizationEventArgsWithPercent("pulling", 0));

            CancelIfException(() =>
                {
                    remoteChuncksForDownload = pull.GetChuncks(credentials.Login, credentials.Password, syncId);
                });

            int i = 1;

            foreach (var chunckId in remoteChuncksForDownload.Select(c => c.Key).ToList())
            {
                //if process is canceled we stop pulling but without exception 
                //in order to move forward and proccess uploaded data
                if (ct.IsCancellationRequested)
                    return;

                try
                {
                    var data = pull.RequestChunck(chunckId, syncId);
                    pullDataProcessor.Save(data, chunckId);
                    remoteChuncksForDownload[chunckId] = true;
                }
                catch
                {
                    //in case of exception we stop pulling but without exception 
                    //in order to move forward and proccess uploaded data
                    return;
                    
                }
                OnStatusChanged(new SynchronizationEventArgsWithPercent("pulling",
                                                                        (i*100)/remoteChuncksForDownload.Count));
                i++;
            }
        }

        private void Push()
        {

            ExitIfCanceled();
            OnStatusChanged(new SynchronizationEventArgsWithPercent("pushing", 0));

            CancelIfException(() =>
                {
                    var dataByChuncks = pushDataProcessor.GetChuncks();
                    int i = 1;
                    foreach (var chunckDescription in dataByChuncks)
                    {
                        ExitIfCanceled();
                        push.PushChunck(chunckDescription.Id, chunckDescription.Content, syncId);
                        pushDataProcessor.MarkChunckAsPushed(chunckDescription.Id);
                        OnStatusChanged(new SynchronizationEventArgsWithPercent("pushing", (i*100)/dataByChuncks.Count));
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
                    credentials = authentificator.RequestCredentials();

                    OnStatusChanged(
                        new SynchronizationEventArgs(string.Format("handshake app {0}, device {1}", appId, androidId)));
                    Thread.Sleep(1000);
                    syncId = handshake.Execute(credentials.Login, credentials.Password, androidId, appId, null);
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
            Validate();
            OnProcessFinished();
        }

        public void Cancel()
        {
            Task.Factory.StartNew(CancelInternal);
        }

        private void CancelInternal()
        {
         
            tokenSource2.Cancel();
            OnProcessCanceling();
            List<Exception> exceptions = new List<Exception>();
            try
            {
                Task.WaitAll(task);
            }
            catch (AggregateException e)
            {
                exceptions = e.InnerExceptions.ToList();
            }

            OnProcessCanceled(exceptions);
        }

        #region events

        public event EventHandler<SynchronizationEventArgs> StatusChanged;
        public event EventHandler ProcessFinished;
        public event EventHandler ProcessCanceling;
        public event EventHandler<SynchronizationCanceledEventArgs> ProcessCanceled;

        protected void OnProcessFinished()
        {
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
            catch
            {
                Cancel();
                throw;
            }
        }

    }
}