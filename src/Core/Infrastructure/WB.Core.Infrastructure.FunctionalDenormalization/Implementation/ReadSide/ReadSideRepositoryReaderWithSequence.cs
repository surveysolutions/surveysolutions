using System;
using Ncqrs;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide
{
    public class ReadSideRepositoryReaderWithSequence<T> : IReadSideRepositoryReader<T> where T : class, IReadSideRepositoryEntity
    {
        private readonly IReadSideRepositoryReader<ViewWithSequence<T>> interviewReader;
        private IEventStore eventStore;
        private Action<Guid, long> additionalEventChecker;

        public ReadSideRepositoryReaderWithSequence(
            IReadSideRepositoryReader<ViewWithSequence<T>> interviewReader, Action<Guid, long> additionalEventChecker)
        {
            this.eventStore = NcqrsEnvironment.Get<IEventStore>();
            this.interviewReader = interviewReader;
            this.additionalEventChecker = additionalEventChecker;
        }

        public int Count()
        {
            return this.interviewReader.Count();
        }

        public T GetById(Guid id)
        {
            var view = this.interviewReader.GetById(id);

            if (this.IsViewWasUpdatedFromEventStream(id, view == null ? 0 : view.Sequence))
            {
                view = this.interviewReader.GetById(id);
                if (view == null)
                    return null;
            }

            return view.Document;
        }

        private bool IsViewWasUpdatedFromEventStream(Guid interviewId, long sequence)
        {
            this.additionalEventChecker(interviewId,sequence);
            var events = this.eventStore.ReadFrom(interviewId, sequence + 1, long.MaxValue);

            if (events.IsEmpty)
                return false;

            var bus = NcqrsEnvironment.Get<IEventBus>() as ViewConstructorEventBus;
            if (bus == null)
                return false;

            bus.PublishForSingleEventSource(events, interviewId);
            return true;
        }
    }
}

