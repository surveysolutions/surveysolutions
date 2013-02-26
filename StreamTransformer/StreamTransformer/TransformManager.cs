using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamTransformer
{
    using Main.Core.Events;
    using Main.Synchronization.SyncManager;
    using Main.Synchronization.SyncSreamProvider;
    using Main.Synchronization.SyncStreamCollector;

    class TransformManager : ISyncManager
    {
        #region Implementation of ISyncManager

        /// <summary>
        /// The event stream provider.
        /// </summary>
        private readonly ISyncEventStreamProvider eventStreamProvider;

        /// <summary>
        /// The stream collector.
        /// </summary>
        private readonly ISyncStreamCollector streamCollector;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransformManager"/> class.
        /// </summary>
        /// <param name="provider">
        /// The provider.
        /// </param>
        /// <param name="collector">
        /// The collector.
        /// </param>
        public TransformManager(ISyncEventStreamProvider provider, ISyncStreamCollector collector)
         {
             this.eventStreamProvider = provider;
             this.streamCollector = collector;
         }

        public bool IsWorking { get; private set; }

        public void StartPull()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The start push.
        /// </summary>
        public void StartPump()
        {
            this.streamCollector.PrepareToCollect();

            int currentChunkSize = Math.Min(this.streamCollector.MaxChunkSize, 1024);
            int counter = 0;

            var chunk = new List<AggregateRootEvent>();

            try
            {
                // read from stream and handle by chunk
                foreach (AggregateRootEvent evnt in this.eventStreamProvider.GetEventStream())
                {
                    this.HandleEvent(chunk, evnt);
                    
                    counter++;

                    if (counter == currentChunkSize)
                    {
                        
                        this.streamCollector.Collect(chunk);
                        chunk = new List<AggregateRootEvent>();
                        counter = 0;
                    }
                }

                // process partial completed chunk
                if (counter > 0)
                {
                    this.streamCollector.Collect(chunk);
                }
                // notify collector about finishing
                this.streamCollector.Finish();

            }
            catch (Exception e)
            {
                // log
                throw;
            }
        }
        
        private void HandleEvent(List<AggregateRootEvent> chunk, AggregateRootEvent evnt)
        {
            chunk.Add(evnt);
        }

        public void StartSynchronization()
        {
            throw new NotImplementedException();
        }

        public void StopProcess()
        {
            throw new NotImplementedException();
        }

        public int? GetCurrentProgress()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
