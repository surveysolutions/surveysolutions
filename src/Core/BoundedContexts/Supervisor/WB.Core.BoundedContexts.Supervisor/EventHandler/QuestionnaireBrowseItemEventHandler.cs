using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure.FunctionalDenormalization;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.EventHandler
{
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
