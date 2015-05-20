extern alias datacollection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.EventHandler;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;

using TemplateImported = datacollection::Main.Core.Events.Questionnaire.TemplateImported;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionnaireDenormalizerTests
{
    internal class when_TemplateImported_event_arrived : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            questionnaireDocument = new QuestionnaireDocument() { PublicKey = questionnaireId };

            questionnaireDocumentVersionedStorageMock = new Mock<IReadSideKeyValueStorage<QuestionnaireDocumentVersioned>>();
            plainQuestionnaireRepositoryMock = new Mock<IPlainQuestionnaireRepository>();

            questionnaireBrowseItemDenormalizer = CreateDenormalizer(
                questionnaireDocumentStorage: questionnaireDocumentVersionedStorageMock.Object, plainQuestionnaireRepository: plainQuestionnaireRepositoryMock.Object);
        };

        Because of = () =>
            questionnaireBrowseItemDenormalizer.Handle(CreatePublishedEvent(questionnaireId, new TemplateImported() { AllowCensusMode = true, Source = questionnaireDocument }));

        It should_questionnaireBrowseItem_be_stored_at_versionedReadSideRepositoryWriter_once = () =>
           questionnaireDocumentVersionedStorageMock.Verify(x => x.Store(Moq.It.Is<QuestionnaireDocumentVersioned>(b => b.Version == 1 && b.Questionnaire.PublicKey == questionnaireDocument.PublicKey), questionnaireId.FormatGuid() + "$1"), Times.Once);

        private static QuestionnaireDenormalizer questionnaireBrowseItemDenormalizer;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static QuestionnaireDocument questionnaireDocument;
        private static Mock<IReadSideKeyValueStorage<QuestionnaireDocumentVersioned>> questionnaireDocumentVersionedStorageMock;
        private static Mock<IPlainQuestionnaireRepository> plainQuestionnaireRepositoryMock;

    }
}
