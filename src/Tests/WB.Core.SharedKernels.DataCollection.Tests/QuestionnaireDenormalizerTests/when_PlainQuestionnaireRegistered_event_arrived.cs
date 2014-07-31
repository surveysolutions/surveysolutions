using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection.EventHandler;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.QuestionnaireDenormalizerTests
{
    internal class when_PlainQuestionnaireRegistered_event_arrived : QuestionnaireDenormalizerTestContext
    {
        Establish context = () =>
        {
            questionnaireDocument = new QuestionnaireDocument() { PublicKey = questionnaireId };

            questionnaireBrowseItemStorageMock = new Mock<IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned>>();
            plainQuestionnaireRepositoryMock = new Mock<IPlainQuestionnaireRepository>();
            plainQuestionnaireRepositoryMock.Setup(x => x.GetQuestionnaireDocument(questionnaireId, 1)).Returns(questionnaireDocument);

            questionnaireBrowseItemDenormalizer = CreateQuestionnaireDenormalizer(questionnaireBrowseItemStorageMock.Object, plainQuestionnaireRepositoryMock.Object);
        };

        Because of = () =>
            questionnaireBrowseItemDenormalizer.Handle(CreatePublishedEvent(questionnaireId, new PlainQuestionnaireRegistered(1, false)));

        It should_questionnaireBrowseItem_be_stored_at_versionedReadSideRepositoryWriter_once = () =>
           questionnaireBrowseItemStorageMock.Verify(x => x.Store(Moq.It.Is<QuestionnaireDocumentVersioned>(b => b.Version == 1 && b.Questionnaire == questionnaireDocument), questionnaireId.FormatGuid()), Times.Once);

        private static QuestionnaireDenormalizer questionnaireBrowseItemDenormalizer;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static QuestionnaireDocument questionnaireDocument;
        private static Mock<IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned>> questionnaireBrowseItemStorageMock;
        private static Mock<IPlainQuestionnaireRepository> plainQuestionnaireRepositoryMock;

    }
}
