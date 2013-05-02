using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamTransformer
{
    using System.IO;

    using Main.Core;
    using Main.Synchronization.SyncManager;
    using Main.Synchronization.SyncSreamProvider;
    using Main.Synchronization.SyncStreamCollector;

    using Ncqrs;
    using Ncqrs.Eventing.Storage;

    using Ncqrs.Eventing.Storage.RavenDB;

    using Raven.Client.Document;

    class Program
    {
        static void Main(string[] args)
        {
            var fileName = string.Format("c:\\temp\\result{0}.zip", DateTime.UtcNow.Ticks);
            
            try
            {
                Init();

                // where to get event stream
                var streamProvider = new AllIntEventsStreamProvider();
                // where to write events
                var collector = new CompressedStreamStreamCollector(Guid.NewGuid());
                // who makes transition
                var mnger = new TransformManager(streamProvider, collector);

                mnger.StartPump();

                File.WriteAllBytes(fileName, collector.GetExportedStream().ToArray());
                
                Console.WriteLine("Done: " + fileName);
            }
            catch (Exception exc)
            {
                Console.WriteLine("Exception: " + exc.Message);
            }


            Console.ReadLine();
        }

        private static void Init()
        {

            // change to your store
            const string Url = "http://localhost:8080";
            var store = new DocumentStore { Url = Url };
            var eventStore = new RavenDBEventStore(store, 1024);
            store.Initialize();

            NcqrsEnvironment.SetDefault(eventStore);
            NcqrsEnvironment.SetDefault<IStreamableEventStore>(eventStore);
        }

    }
}
