using System;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Services;

namespace WB.Core.BoundedContexts.Headquarters.Questionnaires.Denormalizers
{
    internal class VersionedQustionnaireDocumentDenormalizer : BaseDenormalizer, IEventHandler<TemplateImported>,
        IEventHandler<QuestionnaireDeleted>
    {
        private readonly IQuestionnaireCacheInitializer questionnaireCacheInitializer;
        private readonly IReadSideKeyValueStorage<QuestionnaireDocument> documentStorage;

        public VersionedQustionnaireDocumentDenormalizer(IQuestionnaireCacheInitializer questionnaireCacheInitializer,
            IReadSideKeyValueStorage<QuestionnaireDocument> documentStorage)
        {
            if (questionnaireCacheInitializer == null) throw new ArgumentNullException("questionnaireCacheInitializer");

            this.questionnaireCacheInitializer = questionnaireCacheInitializer;
            this.documentStorage = documentStorage;
        }

        public override object[] Writers
        {
            get { return new object[] {  documentStorage}; }
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            QuestionnaireDocument document = evnt.Payload.Source.Clone();
            if (document == null)
                return;

            this.questionnaireCacheInitializer.InitializeQuestionnaireDocumentWithCaches(document);

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