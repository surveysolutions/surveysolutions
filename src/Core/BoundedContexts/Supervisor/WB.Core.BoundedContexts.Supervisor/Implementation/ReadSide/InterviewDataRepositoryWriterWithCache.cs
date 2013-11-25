using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Supervisor.EventHandler;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.Synchronization;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.ReadSide
{
    public class InterviewDataRepositoryWriterWithCache : IReadSideRepositoryReader<InterviewData>
    {
        private IReadSideRepositoryWriter<IntervieweWithSequence> interviewWriter;
        private IEventStore eventStore;
        private IIncomePackagesRepository incomePackages;

        public InterviewDataRepositoryWriterWithCache(
            IReadSideRepositoryWriter<IntervieweWithSequence> interviewWriter,
            IIncomePackagesRepository incomePackages)
        {
            this.eventStore = NcqrsEnvironment.Get<IEventStore>();
            this.interviewWriter = interviewWriter;
            this.incomePackages = incomePackages;
        }

        public int Count()
        {
            throw new NotImplementedException();
        }

        public InterviewData GetById(Guid id)
        {
            var view = interviewWriter.GetById(id);

            if (this.IsViewWasUpdatedFromEventStream(id, view == null ? 0 : view.Sequence))
            {
                view = interviewWriter.GetById(id);
                if (view == null)
                    return null;
            }

            return view.Document;
        }

        public void Remove(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Store(InterviewData view, Guid id)
        {
            throw new NotImplementedException();
        }

        private bool IsViewWasUpdatedFromEventStream(Guid interviewId, long sequence)
        {
            incomePackages.ProcessItem(interviewId, sequence);

            var events = eventStore.ReadFrom(interviewId, sequence + 1, long.MaxValue);

            if (events.IsEmpty)
                return false;

            var bus = NcqrsEnvironment.Get<IEventBus>() as ViewConstructorEventBus;
            if (bus == null)
                return false;

            bus.PublishForSingleEventSource(events, interviewId);
            return true;
        }
    }

    public class IntervieweWithSequence : IView
    {
        public IntervieweWithSequence(InterviewData document, long sequence)
        {
            Document = document;
            Sequence = sequence;
        }

        public InterviewData Document { get; private set; }
        public long Sequence { get; set; }
    }
}

