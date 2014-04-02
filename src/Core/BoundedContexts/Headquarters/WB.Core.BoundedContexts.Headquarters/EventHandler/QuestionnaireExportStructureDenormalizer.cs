using System;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.ReadSide;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
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
            var questionnaireExportStructure = this.exportViewFactory.CreateQuestionnaireExportStructure(evnt.Payload.Source, evnt.EventSequence);
            this.dataExportService.CreateExportedDataStructureByTemplate(questionnaireExportStructure);
            this.readsideRepositoryWriter.Store(questionnaireExportStructure, evnt.EventSourceId);
        }
    }
}
