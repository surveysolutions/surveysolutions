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
        private readonly CompleteQuestionnaireDenormalizer hiddentDenormalizer;
        private readonly IReadSideRepositoryWriter<ZipView> zipWriter;
        private readonly IEventStore eventStore;
        private readonly InProcessEventBus eventEventBus;
        private readonly IStringCompressor compressor;
        private readonly Dictionary<Guid, CompleteQuestionnaireStoreDocument> memcache; 

       // private object locker = new object();

        private int memcacheItemsSizeLimit = 256; //avoid out of memory Exc

        public RavenReadSideRepositoryWriterWithCacheAndZip(
            IReadSideRepositoryWriter<ZipView> zipWriter,
            IReadSideRepositoryWriter<UserDocument> users,
            IStringCompressor comperessor)
        {
            this.eventStore = NcqrsEnvironment.Get<IEventStore>();
            this.zipWriter = zipWriter;
            this.compressor = comperessor;

            this.memcache = new Dictionary<Guid, CompleteQuestionnaireStoreDocument>();
            this.eventEventBus=new InProcessEventBus();
            hiddentDenormalizer = new CompleteQuestionnaireDenormalizer(users);
            
            eventEventBus.RegisterHandler(hiddentDenormalizer, typeof(NewCompleteQuestionnaireCreated));
            eventEventBus.RegisterHandler(hiddentDenormalizer, typeof(CommentSet));
            eventEventBus.RegisterHandler(hiddentDenormalizer, typeof(FlagSet));
            eventEventBus.RegisterHandler(hiddentDenormalizer, typeof(AnswerSet));
            eventEventBus.RegisterHandler(hiddentDenormalizer, typeof(ConditionalStatusChanged));
            eventEventBus.RegisterHandler(hiddentDenormalizer, typeof(PropagatableGroupAdded));
            eventEventBus.RegisterHandler(hiddentDenormalizer, typeof(PropagateGroupCreated));
            eventEventBus.RegisterHandler(hiddentDenormalizer, typeof(PropagatableGroupDeleted));
            eventEventBus.RegisterHandler(hiddentDenormalizer, typeof(QuestionnaireAssignmentChanged));
            eventEventBus.RegisterHandler(hiddentDenormalizer, typeof(QuestionnaireStatusChanged));
            eventEventBus.RegisterHandler(hiddentDenormalizer, typeof(InterviewDeleted));
        }

        public int Count()
        {
            throw new NotImplementedException();
        }

        public CompleteQuestionnaireStoreDocument GetById(Guid id)
        {
            if (!memcache.ContainsKey(id))
            {
         /*       lock (locker)
                {
                    if (!memcache.ContainsKey(id))
                    {*/
                        RestoreFromPersistantStorage(id);
                 /*   }
                }*/
            }
            try
            {
                return memcache[id];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
           
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

            ClearCacheIfLimitExcided();

            var view = RestoreFromEventStream(events, viewItem);
            
            memcache.Add(id, view);
        }

        private void ClearCacheIfLimitExcided()
        {
            if (memcache.Count >= memcacheItemsSizeLimit)
            {
                memcache.Clear();
            }
        }

        private CompleteQuestionnaireStoreDocument RestoreFromEventStream(CommittedEventStream events, ZipView viewItem)
        {
            var view = viewItem == null ? null : compressor.DecompressString<CompleteQuestionnaireStoreDocument>(viewItem.Payload);
            if (!events.IsEmpty)
            {
                using (var entityWriter = new TemporaryViewWriter(view, hiddentDenormalizer))
                {   
                    foreach (var @event in events)
                    {
                        eventEventBus.Publish(@event);
                    }
                    view = entityWriter.GetById(events.SourceId);

                    PersistItem(view, events.SourceId, events.Last().EventSequence);
                }
            }
            return view;
        }

        private void PersistItem(CompleteQuestionnaireStoreDocument view, Guid id, long sequence)
        {
            Task.Factory.StartNew(() =>
                {
                    var zipView = new ZipView(id, compressor.CompressObject(view),sequence);
                    zipWriter.Store(zipView, id);
                });
        }
        
    }
}