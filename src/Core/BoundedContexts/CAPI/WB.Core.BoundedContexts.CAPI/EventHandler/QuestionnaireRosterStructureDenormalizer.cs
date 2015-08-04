using System;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Supervisor.Factories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Capi.EventHandler
{
    public class QuestionnaireRosterStructureDenormalizer : IEventHandler<TemplateImported>, IEventHandler<PlainQuestionnaireRegistered>, IEventHandler<QuestionnaireDeleted>
    {
        private readonly IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnries;
        private readonly IQuestionnaireRosterStructureFactory questionnaireRosterStructureFactory;
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;

        public QuestionnaireRosterStructureDenormalizer(
            IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnries, IQuestionnaireRosterStructureFactory questionnaireRosterStructureFactory, IPlainQuestionnaireRepository plainQuestionnaireRepository)
        {
            this.questionnries = questionnries;
            this.questionnaireRosterStructureFactory = questionnaireRosterStructureFactory;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            Guid id = evnt.EventSourceId;
            long version = evnt.Payload.Version ?? evnt.EventSequence;
            QuestionnaireDocument questionnaireDocument = evnt.Payload.Source;

            this.StoreRosterStructure(id, version, questionnaireDocument);
        }

        public void Handle(IPublishedEvent<PlainQuestionnaireRegistered> evnt)
        {
            Guid id = evnt.EventSourceId;
            long version = evnt.Payload.Version;
            QuestionnaireDocument questionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(id, version);

            this.StoreRosterStructure(id, version, questionnaireDocument);
        }

        private void StoreRosterStructure(Guid id, long version, QuestionnaireDocument questionnaireDocument)
        {
            this.questionnries.AsVersioned().Store(
                this.questionnaireRosterStructureFactory.CreateQuestionnaireRosterStructure(questionnaireDocument, version),
                id.FormatGuid(), version);
        }

        public void Handle(IPublishedEvent<QuestionnaireDeleted> evnt)
        {
            this.questionnries.AsVersioned().Remove(evnt.EventSourceId.FormatGuid(), evnt.Payload.QuestionnaireVersion);
        }
    }
}
