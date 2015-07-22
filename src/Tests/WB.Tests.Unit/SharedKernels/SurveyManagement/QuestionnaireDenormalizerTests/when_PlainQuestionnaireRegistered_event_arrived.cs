using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.EventHandler;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionnaireDenormalizerTests
{
    internal class when_PlainQuestionnaireRegistered_event_arrived : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            questionnaireDocument = new QuestionnaireDocument() { PublicKey = questionnaireId };

            questionnaireBrowseItemStorageMock = new Mock<IReadSideKeyValueStorage<QuestionnaireDocumentVersioned>>();
            plainQuestionnaireRepositoryMock = new Mock<IPlainQuestionnaireRepository>();
            plainQuestionnaireRepositoryMock.Setup(x => x.GetQuestionnaireDocument(questionnaireId, 1)).Returns(questionnaireDocument);

            questionnaireBrowseItemDenormalizer = CreateDenormalizer(
                questionnaireDocumentStorage: questionnaireBrowseItemStorageMock.Object,
                plainQuestionnaireRepository: plainQuestionnaireRepositoryMock.Object);
        };

        Because of = () =>
            questionnaireBrowseItemDenormalizer.Handle(CreatePublishedEvent(questionnaireId, new PlainQuestionnaireRegistered(1, false)));

        It should_questionnaireBrowseItem_be_stored_at_versionedReadSideRepositoryWriter_once = () =>
            questionnaireBrowseItemStorageMock.Verify(
                x => x.Store(
                    Moq.It.Is<QuestionnaireDocumentVersioned>(b => b.Version == 1 && b.Questionnaire.PublicKey == questionnaireDocument.PublicKey),
                    questionnaireId.FormatGuid() + "$1"),
                Times.Once);

        private static QuestionnaireDenormalizer questionnaireBrowseItemDenormalizer;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static QuestionnaireDocument questionnaireDocument;
        private static Mock<IReadSideKeyValueStorage<QuestionnaireDocumentVersioned>> questionnaireBrowseItemStorageMock;
        private static Mock<IPlainQuestionnaireRepository> plainQuestionnaireRepositoryMock;

    }
}
