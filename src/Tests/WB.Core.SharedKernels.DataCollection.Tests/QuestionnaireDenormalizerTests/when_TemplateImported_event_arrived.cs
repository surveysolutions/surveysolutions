using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection.EventHandler;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.QuestionnaireDenormalizerTests
{
    internal class when_TemplateImported_event_arrived : QuestionnaireDenormalizerTestContext
    {
        Establish context = () =>
        {
            questionnaireDocument = new QuestionnaireDocument() { PublicKey = questionnaireId };

            questionnaireDocumentVersionedStorageMock = new Mock<IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned>>();
            plainQuestionnaireRepositoryMock = new Mock<IPlainQuestionnaireRepository>();

            questionnaireBrowseItemDenormalizer = CreateQuestionnaireDenormalizer(questionnaireDocumentVersionedStorageMock.Object, plainQuestionnaireRepositoryMock.Object);
        };

        Because of = () =>
            questionnaireBrowseItemDenormalizer.Handle(CreatePublishedEvent(questionnaireId, new TemplateImported() { AllowCensusMode = true, Source = questionnaireDocument }));

        It should_questionnaireBrowseItem_be_stored_at_versionedReadSideRepositoryWriter_once = () =>
           questionnaireDocumentVersionedStorageMock.Verify(x => x.Store(Moq.It.Is<QuestionnaireDocumentVersioned>(b => b.Version == 1 && b.Questionnaire== questionnaireDocument), questionnaireId.FormatGuid()), Times.Once);

        private static QuestionnaireDenormalizer questionnaireBrowseItemDenormalizer;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static QuestionnaireDocument questionnaireDocument;
        private static Mock<IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned>> questionnaireDocumentVersionedStorageMock;
        private static Mock<IPlainQuestionnaireRepository> plainQuestionnaireRepositoryMock;

    }
}
