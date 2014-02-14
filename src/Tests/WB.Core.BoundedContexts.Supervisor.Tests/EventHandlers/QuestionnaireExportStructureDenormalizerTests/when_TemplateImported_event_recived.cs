﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Supervisor.EventHandler;
using WB.Core.BoundedContexts.Supervisor.Factories;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Supervisor.Tests.EventHandlers.QuestionnaireExportStructureDenormalizerTests
{
    [Subject(typeof (QuestionnaireExportStructureDenormalizer))]
    internal class when_TemplateImported_event_recived
    {
        Establish context = () =>
        {
            questionnaireId = Guid.NewGuid();
            questionnaireDocument = new QuestionnaireDocument() { PublicKey = questionnaireId };
            questionnaireExportStructureMock=new Mock<IVersionedReadSideRepositoryWriter<QuestionnaireExportStructure>>();
            dataExportServiceMock=new Mock<IDataExportService>();
            exportViewFactory=new Mock<IExportViewFactory>();
            questionnaireExportStructureDenormalizer = new QuestionnaireExportStructureDenormalizer(
                questionnaireExportStructureMock.Object, dataExportServiceMock.Object, exportViewFactory.Object);
        };

        Because of = () =>
          questionnaireExportStructureDenormalizer.Handle(CreatePublishableEvent());

        It should_QuestionnaireExportStructure_be_stored_readside = () =>
            questionnaireExportStructureMock.Verify(x => x.Store(Moq.It.IsAny<QuestionnaireExportStructure>(), questionnaireId.FormatGuid()),
                Times.Once());

        It should_QuestionnaireExportStructure_be_stored_by_IDataExportService = () =>
            dataExportServiceMock.Verify(x => x.CreateExportedDataStructureByTemplate(Moq.It.IsAny<QuestionnaireExportStructure>()),
                Times.Once());

        It should_QuestionnaireExportStructure_be_created_by_IExportViewFactory = () =>
            exportViewFactory.Verify(x => x.CreateQuestionnaireExportStructure(questionnaireDocument, QuestionnaireVersion),
                Times.Once());

        protected static IPublishedEvent<TemplateImported> CreatePublishableEvent()
        {
            var publishableEventMock = new Mock<IPublishedEvent<TemplateImported>>();
            publishableEventMock.Setup(x => x.Payload).Returns(new TemplateImported() { Source = questionnaireDocument });
            publishableEventMock.Setup(x => x.EventSourceId).Returns(questionnaireId);
            publishableEventMock.Setup(x => x.EventSequence).Returns(QuestionnaireVersion);
            return publishableEventMock.Object;
        }

        private static QuestionnaireExportStructureDenormalizer questionnaireExportStructureDenormalizer;
        private static Mock<IVersionedReadSideRepositoryWriter<QuestionnaireExportStructure>> questionnaireExportStructureMock;
        private static Mock<IDataExportService> dataExportServiceMock;
        private static Mock<IExportViewFactory> exportViewFactory;
        private static Guid questionnaireId;
        private const long QuestionnaireVersion = 2;
        private static QuestionnaireDocument questionnaireDocument;
    }
}
