using System;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    internal class QuestionnaireExportStructureDenormalizer : BaseDenormalizer, IEventHandler<TemplateImported>, IEventHandler<PlainQuestionnaireRegistered>, IEventHandler<QuestionnaireDeleted>
    {
        private readonly IReadSideKeyValueStorage<QuestionnaireExportStructure> readsideRepositoryWriter;
        private readonly IDataExportRepositoryWriter dataExportWriter;

        private readonly IExportViewFactory exportViewFactory;
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;

        public QuestionnaireExportStructureDenormalizer(
            IReadSideKeyValueStorage<QuestionnaireExportStructure> readsideRepositoryWriter, IDataExportRepositoryWriter dataExportWriter,
            IExportViewFactory exportViewFactory, IPlainQuestionnaireRepository plainQuestionnaireRepository)
        {
            this.readsideRepositoryWriter = readsideRepositoryWriter;
            this.dataExportWriter = dataExportWriter;
            this.exportViewFactory = exportViewFactory;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
        }

        public override object[] Writers
        {
            get { return new object[] { readsideRepositoryWriter, dataExportWriter }; }
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            Guid id = evnt.EventSourceId;
            long version = evnt.Payload.Version ?? evnt.EventSequence;
            QuestionnaireDocument questionnaireDocument = evnt.Payload.Source;

            this.StoreExportStructure(id, version, questionnaireDocument);
        }

        public void Handle(IPublishedEvent<PlainQuestionnaireRegistered> evnt)
        {
            Guid id = evnt.EventSourceId;
            long version = evnt.Payload.Version;
            QuestionnaireDocument questionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(id, version);

            this.StoreExportStructure(id, version, questionnaireDocument);
        }

        public void Handle(IPublishedEvent<QuestionnaireDeleted> evnt)
        {
            this.readsideRepositoryWriter.AsVersioned().Remove(evnt.EventSourceId.FormatGuid(), evnt.Payload.QuestionnaireVersion);
            this.dataExportWriter.DeleteExportedDataForQuestionnaireVersion(evnt.EventSourceId, evnt.Payload.QuestionnaireVersion);
        }

        private void StoreExportStructure(Guid id, long version, QuestionnaireDocument questionnaireDocument)
        {
            var questionnaireExportStructure = this.exportViewFactory.CreateQuestionnaireExportStructure(questionnaireDocument, version);
            this.dataExportWriter.CreateExportStructureByTemplate(questionnaireExportStructure);
            this.readsideRepositoryWriter.AsVersioned().Store(questionnaireExportStructure, id.FormatGuid(), version);
        }
    }
}
