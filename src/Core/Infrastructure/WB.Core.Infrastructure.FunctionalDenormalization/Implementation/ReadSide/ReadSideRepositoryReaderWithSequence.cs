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
        private readonly IReadSideRepositoryReader<ViewWithSequence<T>> interviewReader;
        private readonly IReadSideRepositoryWriter<ViewWithSequence<T>> interviewWriter;
        private static ConcurrentDictionary<Guid, bool> packagesInProcess = new ConcurrentDictionary<Guid, bool>();
        private Action<Guid, long> additionalEventChecker;

        public ReadSideRepositoryReaderWithSequence(
            IReadSideRepositoryReader<ViewWithSequence<T>> interviewReader, Action<Guid, long> additionalEventChecker, IReadSideRepositoryWriter<ViewWithSequence<T>> interviewWriter)
        {
            this.interviewReader = interviewReader;
            this.additionalEventChecker = additionalEventChecker;
            this.interviewWriter = interviewWriter;
        }

        public int Count()
        {
            return this.interviewReader.Count();
        }

        public T GetById(Guid id)
        {
            var view = this.interviewReader.GetById(id);

            if (this.IsViewWasUpdatedFromEventStream(id, view == null ? 0 : view.Sequence, view))
            {
                view = this.interviewReader.GetById(id);
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

            this.WaitUntilViewCanBeProcessed(id);

            this.additionalEventChecker(id, sequence);

            var eventStream = eventStore.ReadFrom(id, sequence + 1, long.MaxValue);

            if (eventStream.IsEmpty)
                return false;

            var inMemoryStorage = new SingleEventSourceStorageStrategy<ViewWithSequence<T>>(view);

            bus.PublishByEventSource(eventStream, inMemoryStorage);

            if (view != null)
                interviewWriter.Store(view, id);
            else
            {
                view = interviewWriter.GetById(id);
                if (view != null)
                    interviewWriter.Remove(id);
            }
            this.ReleaseSpotForOtherThread(id);
            return true;
        }

        private void ReleaseSpotForOtherThread(Guid id)
        {
            bool dummyBool;
            packagesInProcess.TryRemove(id, out dummyBool);
        }

        private void WaitUntilViewCanBeProcessed(Guid id)
        {
            while (!packagesInProcess.TryAdd(id,true))
            {
                Thread.Sleep(1000);
            }
        }
    }
}

