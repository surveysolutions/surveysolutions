using System;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.SharedKernels.DataCollection.EventHandler
{
    public class QuestionnaireDenormalizer : 
        BaseDenormalizer,
        IEventHandler,
        IEventHandler<TemplateImported>
    {
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned> questionnarieStorage;

        public QuestionnaireDenormalizer(IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned> questionnarieStorage)
        {
            this.questionnarieStorage = questionnarieStorage;
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            var template = new QuestionnaireDocumentVersioned()
                {
                    Questionnaire = evnt.Payload.Source,
                    Version = evnt.EventSequence
                };

            questionnarieStorage.Store(template, evnt.EventSourceId);
        }

        public override Type[] BuildsViews
        {
            get { return new Type[]{typeof(QuestionnaireDocumentVersioned)}; }
        }
    }
}