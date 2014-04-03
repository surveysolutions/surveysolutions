using System;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    [Obsolete("Remove it when HQ is a separate application")]
    public class QuestionnaireBrowseItemEventHandler : AbstractFunctionalEventHandler<QuestionnaireBrowseItem>,
        ICreateHandler<QuestionnaireBrowseItem, TemplateImported>
    {
        public QuestionnaireBrowseItemEventHandler(IVersionedReadSideRepositoryWriter<QuestionnaireBrowseItem> readsideRepositoryWriter)
            : base(readsideRepositoryWriter) {}

        public override Type[] UsesViews
        {
            get { return new Type[0]; }
        }

        public QuestionnaireBrowseItem Create(IPublishedEvent<TemplateImported> evnt)
        {
            var document = evnt.Payload.Source;

            return new QuestionnaireBrowseItem(document, evnt.EventSequence);
        }
    }
}
