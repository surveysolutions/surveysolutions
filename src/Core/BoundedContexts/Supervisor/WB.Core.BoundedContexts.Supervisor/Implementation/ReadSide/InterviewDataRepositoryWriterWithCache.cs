using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Supervisor.EventHandler;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.Synchronization;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.ReadSide
{
    public class InterviewDataRepositoryWriterWithCache: IReadSideRepositoryReader<InterviewData>, 
        IReadSideRepositoryWriter<InterviewData>, 
        IReadSideRepositoryCleaner

    {
        private IReadSideRepositoryWriter<IntervieweWithSequence> interviewWriter;
        private readonly IReadSideRepositoryWriter<UserDocument> users;
        private readonly IVersionedReadSideRepositoryWriter<QuestionnairePropagationStructure> qestionnairePropagationStructure;
        private IEventStore eventStore;
        private Dictionary<Guid, IntervieweWithSequence> memcache;
        private IIncomePackagesRepository incomePackages;

        private int memcacheItemsSizeLimit = 256; //avoid out of memory Exc

        public InterviewDataRepositoryWriterWithCache(
            IReadSideRepositoryWriter<IntervieweWithSequence> interviewWriter,
            IReadSideRepositoryWriter<UserDocument> users,
            IVersionedReadSideRepositoryWriter<QuestionnairePropagationStructure> qestionnairePropagationStructure,
            IIncomePackagesRepository incomePackages,
            IReadSideRepositoryCleanerRegistry cleanerRegistry)
        {
            this.eventStore = NcqrsEnvironment.Get<IEventStore>();
            this.interviewWriter = interviewWriter;
            this.incomePackages = incomePackages;
            this.memcache = new Dictionary<Guid, IntervieweWithSequence>();
            this.users = users;
            this.qestionnairePropagationStructure = qestionnairePropagationStructure;
            cleanerRegistry.Register(this);
        }

        public int Count()
        {
            throw new NotImplementedException();
        }

        InterviewData IReadSideRepositoryWriter<InterviewData>.GetById(Guid id)
        {
            if (!memcache.ContainsKey(id))
            {
                RestoreFromPersistantStorage(id);
            }
            try
            {
                return memcache[id].Document;
            }
            catch (KeyNotFoundException)
            {
                return null;
            }

        }

        InterviewData IReadSideRepositoryReader<InterviewData>.GetById(Guid id)
        {
            if (!memcache.ContainsKey(id))
            {
                ((IReadSideRepositoryWriter<InterviewData>)this).GetById(id);
            }

            var view = memcache[id];
            UpdateViewFromEventStream(view);
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

        private void RestoreFromPersistantStorage(Guid id)
        {
            var viewItem = interviewWriter.GetById(id);

            long start = viewItem == null ? 0 : viewItem.Sequence;
            
            var events = eventStore.ReadFrom(id, start, long.MaxValue);

            if (viewItem == null && events.IsEmpty)
                return;

            var maxSequence = viewItem == null ? events.Last().EventSequence : viewItem.Sequence;

            ClearCacheIfLimitExcided();

            var view = viewItem == null ? null : viewItem.Document;
            var updatedView= RestoreFromEventStream(events, view);

            memcache.Add(id, new IntervieweWithSequence(updatedView, maxSequence));
        }

        private void UpdateViewFromEventStream(IntervieweWithSequence viewItem)
        {
            incomePackages.ProcessItem(viewItem.Document.InterviewId, viewItem.Sequence);

            var events = eventStore.ReadFrom(viewItem.Document.InterviewId, viewItem.Sequence, long.MaxValue);
            if (events.IsEmpty)
                return;
            var updatedView = RestoreFromEventStream(events, viewItem.Document);

            memcache[updatedView.InterviewId] = new IntervieweWithSequence(updatedView,
                                                                            events.Last().EventSequence);
        }

        private void ClearCacheIfLimitExcided()
        {
            if (memcache.Count >= memcacheItemsSizeLimit)
            {
                memcache.Clear();
            }
        }

        private InterviewData RestoreFromEventStream(CommittedEventStream events, InterviewData view)
        {
            InterviewData updatedView = view;
            if (!events.IsEmpty)
            {

                using (var entityWriter = new TemporaryInterviewWriter(events.SourceId, updatedView, this.users, this.qestionnairePropagationStructure))
                {
                    entityWriter.PublishEvents(events);
                   
                    updatedView = entityWriter.GetById(events.SourceId);

                    PersistItem(updatedView, events.SourceId, events.Last().EventSequence);
                }
            }
            return updatedView;
        }

        private void PersistItem(InterviewData view, Guid id, long sequence)
        {
            Task.Factory.StartNew(() =>
                {
                    var interviewWithSequence = new IntervieweWithSequence(view, sequence);
                    interviewWriter.Store(interviewWithSequence, id);
                });
        }

      
        public void Clear()
        {
            this.memcache = new Dictionary<Guid, IntervieweWithSequence>();
        }
    }

    public class IntervieweWithSequence:IView
    {
        public IntervieweWithSequence(InterviewData document, long sequence)
        {
            Document = document;
            Sequence = sequence;
        }

        public InterviewData Document { get; private set; }
        public long Sequence { get; private set; }
    }

}
