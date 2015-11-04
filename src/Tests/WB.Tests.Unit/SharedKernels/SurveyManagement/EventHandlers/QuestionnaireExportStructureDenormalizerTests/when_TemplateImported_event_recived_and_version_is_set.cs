﻿extern alias datacollection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

using TemplateImported = datacollection::Main.Core.Events.Questionnaire.TemplateImported;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.QuestionnaireExportStructureDenormalizerTests
{
    [Subject(typeof(QuestionnaireExportStructureDenormalizer))]
    internal class when_TemplateImported_event_recived_and_version_is_set
    {
        Establish context = () =>
        {
            questionnaireId = Guid.NewGuid();
            questionnaireDocument = new QuestionnaireDocument() { PublicKey = questionnaireId };
            questionnaireExportStructureMock = new Mock<IReadSideKeyValueStorage<QuestionnaireExportStructure>>();
            exportViewFactory = new Mock<IExportViewFactory>();

            questionnaireExportStructureDenormalizer = new QuestionnaireExportStructureDenormalizer(
                questionnaireExportStructureMock.Object, exportViewFactory.Object, Mock.Of<IPlainQuestionnaireRepository>(), Mock.Of<IEnvironmentContentService>(), Mock.Of<IFilebasedExportedDataAccessor>(), Mock.Of<IFileSystemAccessor>());
        };

        Because of = () =>
          questionnaireExportStructureDenormalizer.Handle(CreatePublishableEvent());

        It should_QuestionnaireExportStructure_be_stored_readside = () =>
            questionnaireExportStructureMock.Verify(x => x.Store(Moq.It.IsAny<QuestionnaireExportStructure>(), questionnaireId.FormatGuid() + "$" + QuestionnaireVersion.ToString()),
                Times.Once());

        It should_QuestionnaireExportStructure_be_created_by_IExportViewFactory = () =>
            exportViewFactory.Verify(x => x.CreateQuestionnaireExportStructure(questionnaireDocument, QuestionnaireVersion),
                Times.Once());

        protected static IPublishedEvent<TemplateImported> CreatePublishableEvent()
        {
            var publishableEventMock = new Mock<IPublishedEvent<TemplateImported>>();
            publishableEventMock.Setup(x => x.Payload).Returns(new TemplateImported() { Source = questionnaireDocument,Version = QuestionnaireVersion});
            publishableEventMock.Setup(x => x.EventSourceId).Returns(questionnaireId);
            publishableEventMock.Setup(x => x.EventSequence).Returns(1);
            return publishableEventMock.Object;
        }

        private static QuestionnaireExportStructureDenormalizer questionnaireExportStructureDenormalizer;
        private static Mock<IReadSideKeyValueStorage<QuestionnaireExportStructure>> questionnaireExportStructureMock;
        private static Mock<IExportViewFactory> exportViewFactory;
        private static Guid questionnaireId;
        private const long QuestionnaireVersion = 2;
        private static QuestionnaireDocument questionnaireDocument;
    }
}
