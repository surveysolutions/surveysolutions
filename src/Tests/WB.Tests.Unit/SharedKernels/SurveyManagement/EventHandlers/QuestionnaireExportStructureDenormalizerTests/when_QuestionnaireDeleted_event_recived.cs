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
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

using QuestionnaireDeleted = WB.Core.SharedKernels.DataCollection.Events.Questionnaire.QuestionnaireDeleted;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.QuestionnaireExportStructureDenormalizerTests
{
    [Subject(typeof(QuestionnaireExportStructureDenormalizer))]
    internal class when_QuestionnaireDeleted_event_recived
    {
        Establish context = () =>
        {
            questionnaireId = Guid.NewGuid();
            questionnaireExportStructureMock = new Mock<IReadSideKeyValueStorage<QuestionnaireExportStructure>>();
            dataExportService = new Mock<IDataExportRepositoryWriter>();

            questionnaireExportStructureDenormalizer = new QuestionnaireExportStructureDenormalizer(
                questionnaireExportStructureMock.Object, dataExportService.Object, Mock.Of<IExportViewFactory>(), Mock.Of<IPlainQuestionnaireRepository>());
        };

        Because of = () =>
          questionnaireExportStructureDenormalizer.Handle(CreatePublishableEvent());

        It should_QuestionnaireExportStructure_be_deleted_from_readside = () =>
            questionnaireExportStructureMock.Verify(x => x.Remove(questionnaireId.FormatGuid() + "$" + QuestionnaireVersion),
                Times.Once());

        It should_exported_data_be_deleted_by_IDataExportService = () =>
            dataExportService.Verify(x => x.DeleteExportedDataForQuestionnaireVersion(questionnaireId, QuestionnaireVersion),
                Times.Once());

        protected static IPublishedEvent<QuestionnaireDeleted> CreatePublishableEvent()
        {
            var publishableEventMock = new Mock<IPublishedEvent<QuestionnaireDeleted>>();
            publishableEventMock.Setup(x => x.Payload).Returns(new QuestionnaireDeleted() { QuestionnaireVersion = QuestionnaireVersion });
            publishableEventMock.Setup(x => x.EventSourceId).Returns(questionnaireId);
            publishableEventMock.Setup(x => x.EventSequence).Returns(QuestionnaireVersion);
            return publishableEventMock.Object;
        }

        private static QuestionnaireExportStructureDenormalizer questionnaireExportStructureDenormalizer;
        private static Mock<IReadSideKeyValueStorage<QuestionnaireExportStructure>> questionnaireExportStructureMock;

        private static Mock<IDataExportRepositoryWriter> dataExportService;
        private static Guid questionnaireId;
        private const int QuestionnaireVersion = 2;
    }
}
