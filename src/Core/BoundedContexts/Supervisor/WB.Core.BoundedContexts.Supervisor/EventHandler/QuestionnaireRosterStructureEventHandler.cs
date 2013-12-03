using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.FunctionalDenormalization;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.EventHandler
{
    public class QuestionnaireRosterStructureEventHandler : AbstractFunctionalEventHandler<QuestionnaireRosterStructure>,
        ICreateHandler<QuestionnaireRosterStructure, TemplateImported>
    {
        public QuestionnaireRosterStructureEventHandler(IReadSideRepositoryWriter<QuestionnaireRosterStructure> readsideRepositoryWriter)
            : base(readsideRepositoryWriter) {}

        public override Type[] UsesViews
        {
            get { return new Type[0]; }
        }

        public QuestionnaireRosterStructure Create(IPublishedEvent<TemplateImported> evnt)
        {
            return new QuestionnaireRosterStructure(evnt.Payload.Source, evnt.EventSequence);
        }
    }
}
