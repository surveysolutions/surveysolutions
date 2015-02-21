using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Capi.EventHandler;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Capi.QuestionnaireBrowseItemDenormalizerTests
{
    internal class when_QuestionnaireDeleted_event_arrived : QuestionnaireBrowseItemDenormalizerTestContext
    {
        Establish context = () =>
        {
            questionnaireBrowseItemStorageMock = new Mock<IReadSideRepositoryWriter<QuestionnaireBrowseItem>>();
            plainQuestionnaireRepositoryMock = new Mock<IPlainQuestionnaireRepository>();

            questionnaireBrowseItemDenormalizer = CreateQuestionnaireBrowseItemDenormalizer(questionnaireBrowseItemStorageMock.Object, plainQuestionnaireRepositoryMock.Object);
        };

        Because of = () =>
            questionnaireBrowseItemDenormalizer.Handle(CreatePublishedEvent(questionnaireId, new QuestionnaireDeleted() {QuestionnaireVersion = 1}));

        It should_questionnaireBrowseItem_be_removed_from_versionedReadSideRepositoryWriter_once = () =>
           questionnaireBrowseItemStorageMock.Verify(x => x.Remove(questionnaireId.FormatGuid()+"$"+1), Times.Once);

        private static QuestionnaireBrowseItemDenormalizer questionnaireBrowseItemDenormalizer;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static Mock<IReadSideRepositoryWriter<QuestionnaireBrowseItem>> questionnaireBrowseItemStorageMock;
        private static Mock<IPlainQuestionnaireRepository> plainQuestionnaireRepositoryMock;
    }
}
