using System;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Supervisor.Factories;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    public class QuestionnaireRosterStructureEventHandler : AbstractFunctionalEventHandler<QuestionnaireRosterStructure>,
        ICreateHandler<QuestionnaireRosterStructure, TemplateImported>
    {
        private readonly IQuestionnaireRosterStructureFactory questionnaireRosterStructureFactory;

        public QuestionnaireRosterStructureEventHandler(IVersionedReadSideRepositoryWriter<QuestionnaireRosterStructure> readsideRepositoryWriter, IQuestionnaireRosterStructureFactory questionnaireRosterStructureFactory)
            : base(readsideRepositoryWriter)
        {
            this.questionnaireRosterStructureFactory = questionnaireRosterStructureFactory;
        }

        public override Type[] UsesViews
        {
            get { return new Type[0]; }
        }

        public QuestionnaireRosterStructure Create(IPublishedEvent<TemplateImported> evnt)
        {
            return this.questionnaireRosterStructureFactory.CreateQuestionnaireRosterStructure(evnt.Payload.Source, evnt.EventSequence);
        }
    }
}
