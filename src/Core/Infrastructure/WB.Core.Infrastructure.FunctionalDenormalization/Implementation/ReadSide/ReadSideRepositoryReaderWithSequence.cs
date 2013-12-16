using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Ncqrs;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.StorageStrategy;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide
{
    public class ReadSideRepositoryReaderWithSequence<T> : IReadSideRepositoryReader<T> where T : class, IReadSideRepositoryEntity
    {
        private readonly IReadSideRepositoryReader<ViewWithSequence<T>> readsideReader;
        private readonly IReadSideRepositoryWriter<ViewWithSequence<T>> readsideWriter;
        private static ConcurrentDictionary<Guid, bool> packagesInProcess = new ConcurrentDictionary<Guid, bool>();
        private Action<Guid, long> additionalEventChecker;
        private const int CountOfAttempt = 60;

        public ReadSideRepositoryReaderWithSequence(
            IReadSideRepositoryReader<ViewWithSequence<T>> readsideReader, Action<Guid, long> additionalEventChecker, IReadSideRepositoryWriter<ViewWithSequence<T>> readsideWriter)
        {
            this.readsideReader = readsideReader;
            this.additionalEventChecker = additionalEventChecker;
            this.readsideWriter = readsideWriter;
        }

        public int Count()
        {
            return this.readsideReader.Count();
        }

        public T GetById(Guid id)
        {
            var view = this.readsideReader.GetById(id);

            if (this.IsViewWasUpdatedFromEventStream(id, view == null ? 0 : view.Sequence, view))
            {
                view = this.readsideReader.GetById(id);
                if (view == null)
                    return null;
            }

            return view.Document;
        }

        private bool IsViewWasUpdatedFromEventStream(Guid id, long sequence, ViewWithSequence<T> view)
        {
            var bus = NcqrsEnvironment.Get<IEventBus>() as IEventDispatcher;
            if (bus == null)
                return false;
            var eventStore = NcqrsEnvironment.Get<IEventStore>();
            if (eventStore == null)
                return false;
            if (!this.WaitUntilViewCanBeProcessed(id))
                return false;

            var result = false;

            try
            {
                this.additionalEventChecker(id, sequence);

                var eventStream = eventStore.ReadFrom(id, sequence + 1, long.MaxValue);

                if (!eventStream.IsEmpty)
                {
                    using (var inMemoryStorage = new InMemoryViewStorage<ViewWithSequence<T>>(this.readsideWriter, id))
                    {
                        bus.PublishByEventSource(eventStream, inMemoryStorage);
                    }

                    result = true;
                }
            }
            finally
            {
                this.ReleaseSpotForOtherThread(id);
            }

            return result;
        }

        private void ReleaseSpotForOtherThread(Guid id)
        {
            bool dummyBool;
            packagesInProcess.TryRemove(id, out dummyBool);
        }

        private bool WaitUntilViewCanBeProcessed(Guid id)
        {
            int i = 0;
            while (!packagesInProcess.TryAdd(id,true))
            {
                if (i > CountOfAttempt)
                {
                    return false;
                }
                Thread.Sleep(1000);
                i++;
            }
            return true;
        }
    }
}

