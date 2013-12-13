using System;
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

            return true;
        }
    }
}

