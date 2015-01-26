extern alias datacollection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using datacollection::Main.Core.Events.Questionnaire;
using Moq;
using WB.Core.BoundedContexts.Capi.EventHandler;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Capi.QuestionnaireBrowseItemDenormalizerTests
{
    internal class when_TemplateImported_event_arrived : QuestionnaireBrowseItemDenormalizerTestContext
    {
        Establish context = () =>
        {
            questionnaireDocument=new QuestionnaireDocument(){PublicKey = questionnaireId};

            questionnaireBrowseItemStorageMock = new Mock<IReadSideRepositoryWriter<QuestionnaireBrowseItem>>();
            plainQuestionnaireRepositoryMock=new Mock<IPlainQuestionnaireRepository>();

            questionnaireBrowseItemDenormalizer = CreateQuestionnaireBrowseItemDenormalizer(questionnaireBrowseItemStorageMock.Object, plainQuestionnaireRepositoryMock.Object);
        };

        Because of = () =>
            questionnaireBrowseItemDenormalizer.Handle(CreatePublishedEvent(questionnaireId, new TemplateImported() { AllowCensusMode = true, Source = questionnaireDocument }));

        It should_questionnaireBrowseItem_be_stored_at_versionedReadSideRepositoryWriter_once = () =>
           questionnaireBrowseItemStorageMock.Verify(x => x.Store(Moq.It.Is<QuestionnaireBrowseItem>(b => b.AllowCensusMode && b.Version == 1 && b.QuestionnaireId == questionnaireId), questionnaireId.FormatGuid() + "$1"), Times.Once);
        
        private static QuestionnaireBrowseItemDenormalizer questionnaireBrowseItemDenormalizer;
        private static Guid questionnaireId=Guid.Parse("11111111111111111111111111111111");
        private static QuestionnaireDocument questionnaireDocument;
        private static Mock<IReadSideRepositoryWriter<QuestionnaireBrowseItem>> questionnaireBrowseItemStorageMock;
        private static Mock<IPlainQuestionnaireRepository> plainQuestionnaireRepositoryMock;
    }
}
