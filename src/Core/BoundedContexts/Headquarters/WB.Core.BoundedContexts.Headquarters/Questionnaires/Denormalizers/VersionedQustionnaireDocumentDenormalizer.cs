using System;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.Questionnaires.Denormalizers
{
    internal class VersionedQustionnaireDocumentDenormalizer : BaseDenormalizer, 
        IEventHandler<TemplateImported>,
        IEventHandler<QuestionnaireDeleted>
    {
        private readonly IReadSideKeyValueStorage<QuestionnaireDocument> documentStorage;
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;

        public VersionedQustionnaireDocumentDenormalizer(
            IReadSideKeyValueStorage<QuestionnaireDocument> documentStorage, 
            IPlainQuestionnaireRepository plainQuestionnaireRepository)
        {
            this.documentStorage = documentStorage;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
        }

        public override object[] Writers
        {
            get { return new object[] {  documentStorage}; }
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            QuestionnaireDocument document = this.plainQuestionnaireRepository.GetQuestionnaireDocument(evnt.EventSourceId,evnt.Payload.Version.Value).Clone();
            if (document == null)
                return;

            this.documentStorage.Store(document, this.CreateDocumentId(document.PublicKey, evnt.Payload.Version ?? evnt.EventSequence));
        }

        public void Handle(IPublishedEvent<QuestionnaireDeleted> evnt)
        {
            this.documentStorage.Remove(this.CreateDocumentId(evnt.EventSourceId, evnt.Payload.QuestionnaireVersion));

        }

        private string CreateDocumentId(Guid questionnaireId, long questionnaireVersion)
        {
            return questionnaireId.FormatGuid() + "$" + questionnaireVersion;
        }
    }
}