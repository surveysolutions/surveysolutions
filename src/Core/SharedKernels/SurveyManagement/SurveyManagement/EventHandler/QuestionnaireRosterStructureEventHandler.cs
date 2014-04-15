using System;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Supervisor.Factories;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    public class QuestionnaireRosterStructureEventHandler : AbstractFunctionalEventHandler<QuestionnaireRosterStructure>,
        ICreateHandler<QuestionnaireRosterStructure, TemplateImported>,
        ICreateHandler<QuestionnaireRosterStructure, PlainQuestionnaireRegistered>
    {
        private readonly IQuestionnaireRosterStructureFactory questionnaireRosterStructureFactory;
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;

        public QuestionnaireRosterStructureEventHandler(
            IVersionedReadSideRepositoryWriter<QuestionnaireRosterStructure> readsideRepositoryWriter,
            IQuestionnaireRosterStructureFactory questionnaireRosterStructureFactory, IPlainQuestionnaireRepository plainQuestionnaireRepository)
            : base(readsideRepositoryWriter)
        {
            this.questionnaireRosterStructureFactory = questionnaireRosterStructureFactory;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
        }

        public override Type[] UsesViews
        {
            get { return new Type[0]; }
        }

        public QuestionnaireRosterStructure Create(IPublishedEvent<TemplateImported> evnt)
        {
            long version = evnt.EventSequence;
            QuestionnaireDocument questionnaireDocument = evnt.Payload.Source;

            return this.questionnaireRosterStructureFactory.CreateQuestionnaireRosterStructure(questionnaireDocument, version);
        }

        public QuestionnaireRosterStructure Create(IPublishedEvent<PlainQuestionnaireRegistered> evnt)
        {
            Guid id = evnt.EventSourceId;
            long version = evnt.Payload.Version;
            QuestionnaireDocument questionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(id, version);

            return this.questionnaireRosterStructureFactory.CreateQuestionnaireRosterStructure(questionnaireDocument, version);
        }
    }
}
