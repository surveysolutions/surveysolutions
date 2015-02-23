using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.DenormalizerStorage;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.SyncStorage;
using WB.Tests.CleanIntegration.EsentTests;

namespace WB.Tests.CleanIntegration.Synchronization.Concurrency
{
    [Subject(typeof(ReadSideChunkWriter))]
    internal class when_multiple_threads_saving_chunks : with_esent_store<SynchronizationDeltasCounter>
    {
        Establish context = () =>
        {
            itemsToStore = new List<Tuple<SyncItem, DateTime>>();
            syncItemsCount = 300;

            var timeStamp = new DateTime(2010, 1, 1);
            userId = Guid.Parse("11111111111111111111111111111111");
            for (int i = 0; i < syncItemsCount; i++)
            {
                var item = Tuple.Create(new SyncItem{RootId = Guid.NewGuid()}, timeStamp);
                itemsToStore.Add(item);

                timeStamp = timeStamp.AddDays(1);
            }

            metaWriter = new InMemoryReadSideRepositoryAccessor<SynchronizationDeltaMetaInformation>();

            storedMetas = new List<SynchronizationDeltaMetaInformation>();


            var deltaContentStore = new InMemoryReadSideRepositoryAccessor<SynchronizationDeltaContent>();
            writer = Create.ReadSideChunkWriter(metaWriter, deltaContentStore, storage);
        };

        Because of = () =>
        {
            var tasks = new List<Task>();
            foreach (var item in itemsToStore)
            {
                tasks.Add(Task.Run(() =>
                {
                    writer.StoreChunk(item.Item1, userId, item.Item2);
                }));
            }

            Task.WaitAll(tasks.ToArray());
        };

        It should_give_unique_sort_indexes_for_chunks = () =>
            metaWriter.QueryAll(null).Select(x => x.SortIndex).Distinct().Count().ShouldEqual(syncItemsCount);

        It should_provide_correct_sort_index_to_last_item = () =>
            metaWriter.QueryAll(null).Max(x => x.SortIndex).ShouldEqual(syncItemsCount - 1);

        static Guid userId;
        static List<Tuple<SyncItem, DateTime>> itemsToStore;
        static ReadSideChunkWriter writer;
        static InMemoryReadSideRepositoryAccessor<SynchronizationDeltaMetaInformation> metaWriter;
        static List<SynchronizationDeltaMetaInformation> storedMetas;
        static int syncItemsCount;
    }
}

