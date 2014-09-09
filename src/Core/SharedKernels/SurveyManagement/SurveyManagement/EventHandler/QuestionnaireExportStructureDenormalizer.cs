using System;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.QuestionnaireVerification.Services;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    public class QuestionnaireExportStructureDenormalizer : IEventHandler<TemplateImported>, IEventHandler<PlainQuestionnaireRegistered>, IEventHandler<QuestionnaireDeleted>, IEventHandler
    {
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireExportStructure> readsideRepositoryWriter;
        private readonly IQuestionnaireUpgradeService questionnaireUpgradeService;
        private readonly IDataExportService dataExportService;
        private readonly IExportViewFactory exportViewFactory;
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;

        public QuestionnaireExportStructureDenormalizer(
            IVersionedReadSideRepositoryWriter<QuestionnaireExportStructure> readsideRepositoryWriter, IDataExportService dataExportService,
            IExportViewFactory exportViewFactory, IPlainQuestionnaireRepository plainQuestionnaireRepository,
            IQuestionnaireUpgradeService questionnaireUpgradeService)
        {
            this.readsideRepositoryWriter = readsideRepositoryWriter;
            this.dataExportService = dataExportService;
            this.exportViewFactory = exportViewFactory;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.questionnaireUpgradeService = questionnaireUpgradeService;
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
            this.readsideRepositoryWriter.Remove(evnt.EventSourceId, evnt.Payload.QuestionnaireVersion);
            this.dataExportService.DeleteExportedDataForQuestionnaireVersion(evnt.EventSourceId, evnt.Payload.QuestionnaireVersion);
        }

        private void StoreExportStructure(Guid id, long version, QuestionnaireDocument questionnaireDocument)
        {
            questionnaireDocument = questionnaireUpgradeService.CreateRostersVariableName(questionnaireDocument);
            var questionnaireExportStructure = this.exportViewFactory.CreateQuestionnaireExportStructure(questionnaireDocument, version);
            this.dataExportService.CreateExportedDataStructureByTemplate(questionnaireExportStructure);
            this.readsideRepositoryWriter.Store(questionnaireExportStructure, id);
        }
    }
}
