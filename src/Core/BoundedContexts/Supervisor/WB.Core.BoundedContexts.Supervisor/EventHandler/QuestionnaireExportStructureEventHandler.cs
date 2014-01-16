using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.EventHandler
{
    public class QuestionnaireExportStructureEventHandler : AbstractFunctionalEventHandler<QuestionnaireExportStructure>,
        ICreateHandler<QuestionnaireExportStructure, TemplateImported>
    {
        public QuestionnaireExportStructureEventHandler(IVersionedReadSideRepositoryWriter<QuestionnaireExportStructure> readsideRepositoryWriter)
            : base(readsideRepositoryWriter)
        {
        }

        public override Type[] UsesViews
        {
            get { return new Type[0]; }
        }

        public QuestionnaireExportStructure Create(IPublishedEvent<TemplateImported> evnt)
        {
            return new QuestionnaireExportStructure(evnt.Payload.Source, evnt.EventSequence);
        }
    }
}
