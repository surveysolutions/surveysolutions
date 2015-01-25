using System;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Capi.EventHandler
{
    public class QuestionnaireDenormalizer : 
        BaseDenormalizer,
        IEventHandler<TemplateImported>, IEventHandler<QuestionnaireDeleted>,
        IEventHandler<PlainQuestionnaireRegistered>
    {
        private readonly IVersionedReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnarieStorage;
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;

        public QuestionnaireDenormalizer(IVersionedReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnarieStorage, 
            IPlainQuestionnaireRepository plainQuestionnaireRepository)
        {
            this.questionnarieStorage = questionnarieStorage;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            Guid id = evnt.EventSourceId;
            long version = evnt.Payload.Version ?? evnt.EventSequence;
            QuestionnaireDocument questionnaireDocument = evnt.Payload.Source;

            this.StoreQuestionnaireDocument(id, version, questionnaireDocument);
        }

        public void Handle(IPublishedEvent<QuestionnaireDeleted> evnt)
        {
            this.questionnarieStorage.Remove(evnt.EventSourceId, evnt.Payload.QuestionnaireVersion);
        }

        public void Handle(IPublishedEvent<PlainQuestionnaireRegistered> evnt)
        {
            Guid id = evnt.EventSourceId;
            long version = evnt.Payload.Version;
            QuestionnaireDocument questionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(id, version);

            this.StoreQuestionnaireDocument(id, version, questionnaireDocument);
        }

        private void StoreQuestionnaireDocument(Guid id, long version, QuestionnaireDocument questionnaireDocument)
        {
            var template = new QuestionnaireDocumentVersioned
            {
                Questionnaire = questionnaireDocument,
                Version = version
            };

            this.questionnarieStorage.Store(template, id);
        }

        public override object[] Writers
        {
            get { return new[] { this.questionnarieStorage }; }
        }
    }
}