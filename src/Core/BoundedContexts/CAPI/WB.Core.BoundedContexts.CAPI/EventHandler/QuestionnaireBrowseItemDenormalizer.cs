using System;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Capi.EventHandler
{
    public class QuestionnaireBrowseItemDenormalizer : IEventHandler<TemplateImported>, IEventHandler<PlainQuestionnaireRegistered>, IEventHandler<QuestionnaireDeleted>
    {
        private readonly IReadSideRepositoryWriter<QuestionnaireBrowseItem> documentStorage;
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;

        public QuestionnaireBrowseItemDenormalizer(IReadSideRepositoryWriter<QuestionnaireBrowseItem> documentStorage, 
            IPlainQuestionnaireRepository plainQuestionnaireRepository)
        {
            this.documentStorage = documentStorage;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            Guid id = evnt.EventSourceId;
            long version = evnt.Payload.Version ?? evnt.EventSequence;
            QuestionnaireDocument questionnaireDocument = evnt.Payload.Source;

            this.StoreBrowseItem(id, version, questionnaireDocument, evnt.Payload.AllowCensusMode);
        }

        public void Handle(IPublishedEvent<PlainQuestionnaireRegistered> evnt)
        {
            Guid id = evnt.EventSourceId;
            long version = evnt.Payload.Version;
            QuestionnaireDocument questionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(id, version);

            this.StoreBrowseItem(id, version, questionnaireDocument, evnt.Payload.AllowCensusMode);
        }

        private void StoreBrowseItem(Guid id, long version, QuestionnaireDocument document, bool allowCensusMode)
        {
            var browseItem = new QuestionnaireBrowseItem(document, version, allowCensusMode);
            this.documentStorage.AsVersioned().Store(browseItem, id.FormatGuid(), version);
        }

        public void Handle(IPublishedEvent<QuestionnaireDeleted> evnt)
        {
            this.documentStorage.AsVersioned().Remove(evnt.EventSourceId.FormatGuid(), evnt.Payload.QuestionnaireVersion);
        }
    }
}
