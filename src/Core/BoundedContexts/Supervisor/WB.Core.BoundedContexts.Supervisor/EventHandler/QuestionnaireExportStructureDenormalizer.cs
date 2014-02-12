using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Supervisor.Factories;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.Infrastructure.FunctionalDenormalization;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.EventHandler
{
    public class QuestionnaireExportStructureDenormalizer : IEventHandler<TemplateImported>, IEventHandler
    {
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireExportStructure> readsideRepositoryWriter;
        private readonly IDataExportService dataExportService;
        private readonly IExportViewFactory exportViewFactory;

        public QuestionnaireExportStructureDenormalizer(
            IVersionedReadSideRepositoryWriter<QuestionnaireExportStructure> readsideRepositoryWriter, IDataExportService dataExportService,
            IExportViewFactory exportViewFactory)
        {
            this.readsideRepositoryWriter = readsideRepositoryWriter;
            this.dataExportService = dataExportService;
            this.exportViewFactory = exportViewFactory;
        }

        public string Name
        {
            get { return this.GetType().Name; }
        }

        public Type[] UsesViews
        {
            get { return new Type[0]; }
        }

        public Type[] BuildsViews { get { return new [] { typeof(QuestionnaireExportStructure) }; } }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            var questionnaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(evnt.Payload.Source, evnt.EventSequence);
            this.dataExportService.CreateExportedDataStructureByTemplate(questionnaireExportStructure);
            this.readsideRepositoryWriter.Store(questionnaireExportStructure, evnt.EventSourceId.ToString());
        }
    }
}
