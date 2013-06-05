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
using CAPI.Android.Syncronization.Pull;
using CAPI.Android.Utils;
using Main.Synchronization.Credentials;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;

namespace CAPI.Android.Syncronization
{
    public class SynchronozationProcessor
    {
        private const int millisecondsTimeout = 1000;
        private const int chunkCount = 10;
        private readonly Context context;
        private CancellationToken ct;
        private CancellationTokenSource tokenSource2;
        private Task task;
        
        private readonly  RestPuller puller;
        private readonly PulledDataProcessor pulledDataProcessor;
        private readonly Dictionary<Guid, bool> remoteChuncksForDownload = new Dictionary<Guid, bool>(); 

        public SynchronozationProcessor(Context context, ISyncAuthenticator authentificator, IChangeLogManipulator changelog)
        {
            this.context = context;
            
            Preparation();
            puller = new RestPuller(SettingsManager.GetSyncAddressPoint(), authentificator);
            pulledDataProcessor = new PulledDataProcessor(changelog, NcqrsEnvironment.Get<ICommandService>());
        }

        #region operations

        private void Validate()
        {
            OnStatusChanged(new SynchronizationEvent("validating"));
            foreach (var chunck in remoteChuncksForDownload.Where(c=>c.Value))
            {
                pulledDataProcessor.Proccess(chunck.Key);
            }
        }

        private void Pull()
        {
            int i = 0;
            foreach (var chunckId in remoteChuncksForDownload.Keys)
            {
                ExitIfCanceled();
                var data = puller.RequestChunck(chunckId);
                pulledDataProcessor.Save(data, chunckId);
                remoteChuncksForDownload[chunckId] = true;
                OnStatusChanged(new SynchronizationEventWithPercent("pulling",
                                                                    (int) ((i/remoteChuncksForDownload.Count)*100)));
                i++;
            }
        }

        private void Push()
        {
            ExitIfCanceled();
            for (int i = 0; i < chunkCount; i++)
            {
                OnStatusChanged(new SynchronizationEventWithPercent("pushing", i*chunkCount));
                Thread.Sleep(millisecondsTimeout);
            }
        }

        private void Handshake()
        {
            ExitIfCanceled();
            OnStatusChanged(new SynchronizationEvent("handshake"));
            for (int i = 0; i < 3; i++)
            {
                remoteChuncksForDownload.Add(Guid.NewGuid(), false);
            }
            Thread.Sleep(millisecondsTimeout);
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