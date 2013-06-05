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
        private CancellationToken ct;
        private CancellationTokenSource tokenSource2;
        private Task task;
        

        public SynchronozationProcessor(Context context)
        {
            this.context = context;
            Preparation();
        }

        #region operations

        private void Validate()
        {
            ExitIfCanceled();
            OnStatusChanged(new SynchronizationEvent("validating"));
            Thread.Sleep(millisecondsTimeout);
            
        }

        private void Pull()
        {
            for (int i = 0; i < chunkCount; i++)
            {
                ExitIfCanceled();
                OnStatusChanged(new SynchronizationEventWithPercent("pulling", i*chunkCount));
                Thread.Sleep(millisecondsTimeout);
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
            Thread.Sleep(millisecondsTimeout);
        }

        #endregion

        public void Run()
        {
            tokenSource2 = new CancellationTokenSource();
            ct = tokenSource2.Token;
            
            task = Task.Factory.StartNew(() =>
                {
                    Handshake();
                    Push();
                    Pull();
                    Validate();
                    OnProcessFinished();
                }, ct);
        }

        public void Cancel()
        {
            

            Task.Factory.StartNew(() =>
                {
                    OnStatusChanged(new SynchronizationEvent("Synchronization is canceling"));
                    tokenSource2.Cancel();

                    try
                    {
                        Task.WaitAll(task);
                    }
                    catch (AggregateException e)
                    {
                        // For demonstration purposes, show the OCE message. 
                       /* foreach (var v in e.InnerExceptions)
                            OnStatusChanged(new SynchronizationEvent(v.Message));*/
                        //Console.WriteLine("msg: " + v.Message);
                    }

                    OnProcessCanceled();
                });
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