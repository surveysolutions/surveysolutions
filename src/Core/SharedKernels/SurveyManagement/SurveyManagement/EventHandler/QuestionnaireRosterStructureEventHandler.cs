using System;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Supervisor.Factories;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    public class QuestionnaireRosterStructureEventHandler : BaseDenormalizer, IEventHandler<TemplateImported>, IEventHandler<PlainQuestionnaireRegistered>, IEventHandler<QuestionnaireDeleted> 
    {
        private readonly IQuestionnaireRosterStructureFactory questionnaireRosterStructureFactory;
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;
        private readonly IReadSideKeyValueStorage<QuestionnaireRosterStructure> readsideRepositoryWriter;
        
        public QuestionnaireRosterStructureEventHandler(
            IReadSideKeyValueStorage<QuestionnaireRosterStructure> readsideRepositoryWriter,
            IQuestionnaireRosterStructureFactory questionnaireRosterStructureFactory, IPlainQuestionnaireRepository plainQuestionnaireRepository)
        {
            this.questionnaireRosterStructureFactory = questionnaireRosterStructureFactory;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.readsideRepositoryWriter = readsideRepositoryWriter;
        }

        public override object[] Writers
        {
            get { return new[] { readsideRepositoryWriter }; }
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            long version = evnt.Payload.Version ?? evnt.EventSequence;
            QuestionnaireDocument questionnaireDocument = evnt.Payload.Source;

            var view = this.questionnaireRosterStructureFactory.CreateQuestionnaireRosterStructure(questionnaireDocument, version);
            readsideRepositoryWriter.AsVersioned().Store(view, evnt.EventSourceId.FormatGuid(), version);
        }

        public void Handle(IPublishedEvent<PlainQuestionnaireRegistered> evnt)
        {
            Guid id = evnt.EventSourceId;
            long version = evnt.Payload.Version;
            QuestionnaireDocument questionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(id, version);

            var view = this.questionnaireRosterStructureFactory.CreateQuestionnaireRosterStructure(questionnaireDocument, version);
            readsideRepositoryWriter.AsVersioned().Store(view, evnt.EventSourceId.FormatGuid(), version);
        }

        public void Handle(IPublishedEvent<QuestionnaireDeleted> evnt)
        {
           readsideRepositoryWriter.AsVersioned().Remove(evnt.EventSourceId.FormatGuid(), evnt.Payload.QuestionnaireVersion);
        }
    }
}
