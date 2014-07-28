using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Moq;
using WB.Core.BoundedContexts.Capi.EventHandler;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Capi.Tests.QuestionnaireBrowseItemDenormalizerTests
{
    internal class when_PlainQuestionnaireRegistered_event_arrived : QuestionnaireBrowseItemDenormalizerTestContext
    {
        Establish context = () =>
        {
            questionnaireDocument = new QuestionnaireDocument() { PublicKey = questionnaireId };

            questionnaireBrowseItemStorageMock = new Mock<IVersionedReadSideRepositoryWriter<QuestionnaireBrowseItem>>();
            plainQuestionnaireRepositoryMock = new Mock<IPlainQuestionnaireRepository>();
            plainQuestionnaireRepositoryMock.Setup(x => x.GetQuestionnaireDocument(questionnaireId, 1)).Returns(questionnaireDocument);

            questionnaireBrowseItemDenormalizer = CreateQuestionnaireBrowseItemDenormalizer(questionnaireBrowseItemStorageMock.Object, plainQuestionnaireRepositoryMock.Object);
        };

        Because of = () =>
            questionnaireBrowseItemDenormalizer.Handle(CreatePublishedEvent(questionnaireId, new PlainQuestionnaireRegistered(1,false)));

        It should_questionnaireBrowseItem_be_stored_at_versionedReadSideRepositoryWriter_once = () =>
           questionnaireBrowseItemStorageMock.Verify(x => x.Store(Moq.It.Is<QuestionnaireBrowseItem>(b => !b.AllowCensusMode && b.Version == 1 && b.QuestionnaireId == questionnaireId), questionnaireId.FormatGuid()), Times.Once);

        private static QuestionnaireBrowseItemDenormalizer questionnaireBrowseItemDenormalizer;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static QuestionnaireDocument questionnaireDocument;
        private static Mock<IVersionedReadSideRepositoryWriter<QuestionnaireBrowseItem>> questionnaireBrowseItemStorageMock;
        private static Mock<IPlainQuestionnaireRepository> plainQuestionnaireRepositoryMock;
    }
}
