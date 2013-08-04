using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire.Completed;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Utils.Compression;

namespace WB.Supervisor.CompleteQuestionnaireDenormalizer
{
    internal class RavenReadSideRepositoryWriterWithCacheAndZip : IReadSideRepositoryReader<CompleteQuestionnaireStoreDocument>, IReadSideRepositoryWriter<CompleteQuestionnaireStoreDocument>

    {
        private CompleteQuestionnaireDenormalizer hiddentDenormalizer;
        private IReadSideRepositoryWriter<ZipView> zipWriter;
        private IEventStore eventStore;
        private InProcessEventBus eventEventBus;
        private IStringCompressor compressor;
        private Dictionary<Guid, QuestionnarieWithSequence> memcache; 


        private int memcacheItemsSizeLimit = 256; //avoid out of memory Exc

        public RavenReadSideRepositoryWriterWithCacheAndZip(
            IReadSideRepositoryWriter<ZipView> zipWriter,
            IReadSideRepositoryWriter<UserDocument> users,
            IStringCompressor comperessor)
        {
            this.eventStore = NcqrsEnvironment.Get<IEventStore>();
            this.zipWriter = zipWriter;
            this.compressor = comperessor;

            this.memcache = new Dictionary<Guid, QuestionnarieWithSequence>();
            this.eventEventBus = new InProcessEventBus();
            this.hiddentDenormalizer = new CompleteQuestionnaireDenormalizer(users);
            
            RegisterCompleteQuestionnarieDenormalizerAtProcessBus();
        }

        private void RegisterCompleteQuestionnarieDenormalizerAtProcessBus()
        {
            IEnumerable<Type> ieventHandlers =
                hiddentDenormalizer.GetType()
                                   .GetInterfaces()
                                   .Where(
                                       type =>
                                       type.IsInterface && type.IsGenericType &&
                                       type.GetGenericTypeDefinition() == typeof (IEventHandler<>));
            foreach (Type ieventHandler in ieventHandlers)
            {
                eventEventBus.RegisterHandler(hiddentDenormalizer, ieventHandler.GetGenericArguments()[0]);
            }
        }

        public int Count()
        {
            throw new NotImplementedException();
        }

        CompleteQuestionnaireStoreDocument IReadSideRepositoryWriter<CompleteQuestionnaireStoreDocument>.GetById(Guid id)
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

        CompleteQuestionnaireStoreDocument IReadSideRepositoryReader<CompleteQuestionnaireStoreDocument>.GetById(Guid id)
        {
            if (!memcache.ContainsKey(id))
            {
                return ((IReadSideRepositoryWriter<CompleteQuestionnaireStoreDocument>) this).GetById(id);
            }

            var view = memcache[id];
            UpdateViewFromEventStrean(view);
            return view.Document;
        }

        public void Remove(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Store(CompleteQuestionnaireStoreDocument view, Guid id)
        {
            throw new NotImplementedException();
        }

        private void RestoreFromPersistantStorage(Guid id)
        {
            var viewItem = zipWriter.GetById(id);

            long start = viewItem == null ? 0 : viewItem.Sequence;
            
            var events = eventStore.ReadFrom(id, start, long.MaxValue);

            if (viewItem == null && events.IsEmpty)
                return;

            var maxSequence = viewItem == null ? events.Last().EventSequence : viewItem.Sequence;

            ClearCacheIfLimitExcided();

            var view = viewItem == null ? null : compressor.DecompressString<CompleteQuestionnaireStoreDocument>(viewItem.Payload);
            var updatedView= RestoreFromEventStream(events, view);

            memcache.Add(id, new QuestionnarieWithSequence(updatedView, maxSequence));
        }

        private void UpdateViewFromEventStrean(QuestionnarieWithSequence viewItem)
        {
            var events = eventStore.ReadFrom(viewItem.Document.PublicKey, viewItem.Sequence, long.MaxValue);
            if (events.IsEmpty)
                return;
            var updatedView = RestoreFromEventStream(events, viewItem.Document);
            memcache[updatedView.PublicKey] = new QuestionnarieWithSequence(updatedView,
                                                                                  events.Last().EventSequence);
        }

        private void ClearCacheIfLimitExcided()
        {
            if (memcache.Count >= memcacheItemsSizeLimit)
            {
                memcache.Clear();
            }
        }

        private CompleteQuestionnaireStoreDocument RestoreFromEventStream(CommittedEventStream events, CompleteQuestionnaireStoreDocument view)
        {
            CompleteQuestionnaireStoreDocument updatedView = view;
            if (!events.IsEmpty)
            {
                using (var entityWriter = new TemporaryViewWriter(events.SourceId, updatedView, hiddentDenormalizer))
                {   
                    foreach (var @event in events)
                    {
                        eventEventBus.Publish(@event);
                    }
                    updatedView = entityWriter.GetById(events.SourceId);

                    PersistItem(updatedView, events.SourceId, events.Last().EventSequence);
                }
            }
            return updatedView;
        }

        private void PersistItem(CompleteQuestionnaireStoreDocument view, Guid id, long sequence)
        {
            Task.Factory.StartNew(() =>
                {
                    var zipView = new ZipView(id, compressor.CompressObject(view),sequence);
                    zipWriter.Store(zipView, id);
                });
        }

        class QuestionnarieWithSequence
        {
            public QuestionnarieWithSequence(CompleteQuestionnaireStoreDocument document, long sequence)
            {
                Document = document;
                Sequence = sequence;
            }

            public CompleteQuestionnaireStoreDocument Document { get; private set; }
            public long Sequence { get; private set; }
        }
    }
}