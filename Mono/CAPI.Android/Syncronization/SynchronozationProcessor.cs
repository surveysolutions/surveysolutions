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
using CAPI.Android.Settings;
using CAPI.Android.Utils;

namespace CAPI.Android.Syncronization
{
    public class SynchronozationProcessor
    {
        private const int millisecondsTimeout = 2000;
        private const int chunkCount = 10;
        private readonly Context context;
        public bool IsRunning { get; private set; }

        public SynchronozationProcessor(Context context)
        {
            this.context = context;
            Preparation();
        }

        public event EventHandler<SynchronizationEvent> StatusChanged;
        public event EventHandler ProcessFinished;
 
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

        public void Run()
        {
            ThreadPool.QueueUserWorkItem(
                state => RunInternal());
        }
        protected void RunInternal()
        {
            IsRunning = true;
            OnStatusChanged(new SynchronizationEvent("handshake"));
            Thread.Sleep(millisecondsTimeout);

            for (int i = 0; i < chunkCount; i++)
            {
                OnStatusChanged(new SynchronizationEventWithPercent("pushing", i*chunkCount));
                Thread.Sleep(millisecondsTimeout);
            }
            for (int i = 0; i < chunkCount; i++)
            {
                OnStatusChanged(new SynchronizationEventWithPercent("pulling", i * chunkCount));
                Thread.Sleep(millisecondsTimeout);
            }

            OnStatusChanged(new SynchronizationEvent("validating"));
            Thread.Sleep(millisecondsTimeout);
            IsRunning = false;
            OnProcessFinished();
        }

        protected void OnProcessFinished()
        {
            var handler = ProcessFinished;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        protected void OnStatusChanged(SynchronizationEvent evt)
        {
            var handler = StatusChanged;
            if (handler != null)
                handler(this, evt);
        }
    }
}