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

        public SynchronozationProcessor(Context context, ISyncAuthenticator authentificator, IChangeLogManipulator changelog)
        {
            this.context = context;
            
            Preparation();

            pull = new RestPull(SettingsManager.GetSyncAddressPoint(), authentificator);
            push=new RestPush();
            handshake = new RestHandshake();

            pullDataProcessor = new PullDataProcessor(changelog, NcqrsEnvironment.Get<ICommandService>());
            pushDataProcessor = new PushDataProcessor();
        }

        #region operations

        private void Validate()
        {
            OnStatusChanged(new SynchronizationEvent("validating"));

            foreach (var chunck in remoteChuncksForDownload.Where(c=>c.Value).Select(c=>c.Key).ToList())
            {
                pullDataProcessor.Proccess(chunck);
            }
        }

        private void Pull()
        {
            ExitIfCanceled();
            OnStatusChanged(new SynchronizationEventWithPercent("pulling", 0));
            int i = 1;
            foreach (var chunckId in remoteChuncksForDownload.Select(c=>c.Key).ToList())
            {
                //if process is canceled we stop pulling but without exception 
                //in order to move forward and proccess uploaded data
                if(ct.IsCancellationRequested)
                    return;

                var data = pull.RequestChunck(chunckId);
                pullDataProcessor.Save(data, chunckId);
                remoteChuncksForDownload[chunckId] = true;
                OnStatusChanged(new SynchronizationEventWithPercent("pulling",
                                                                    (i*100)/remoteChuncksForDownload.Count));
                i++;
            }
        }

        private void Push()
        {
            ExitIfCanceled();
            OnStatusChanged(new SynchronizationEventWithPercent("pushing", 0));
            var dataByChuncks = pushDataProcessor.GetChuncks();
            int i = 1;
            foreach (var chunckDescription in dataByChuncks)
            {
                ExitIfCanceled();
                push.PushChunck(chunckDescription.Id,chunckDescription.Content);
                pushDataProcessor.MarkChunckAsPushed(chunckDescription.Id);
                OnStatusChanged(new SynchronizationEventWithPercent("pushing", (i*100)/dataByChuncks.Count));
                i++;
            }
        }

        private void Handshake()
        {
            ExitIfCanceled();
            OnStatusChanged(new SynchronizationEvent("handshake"));
            remoteChuncksForDownload = handshake.GetChuncks();
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
            OnStatusChanged(new SynchronizationEvent("Synchronization is canceling"));
            tokenSource2.Cancel();

            try
            {
                Task.WaitAll(task);
            }
            catch (AggregateException e)
            {
            }

            OnProcessCanceled();
        }

        #region events

        public event EventHandler<SynchronizationEvent> StatusChanged;
        public event EventHandler ProcessFinished;
        public event EventHandler ProcessCanceled;

        protected void OnProcessFinished()
        {
            var handler = ProcessFinished;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        protected void OnProcessCanceled()
        {
            var handler = ProcessCanceled;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        protected void OnStatusChanged(SynchronizationEvent evt)
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
 
    }
}