using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using NSubstitute;
using WB.Core.BoundedContexts.Supervisor.Questionnaires;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Atom;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Synchronization.QuestionnaireSynchronizerTests
{
    internal class when_pull_questionnaire_templates : QuestionnaireSynchronizerTestContext
    {
        Establish context = () =>
        {
            emptyLocalQuestionnaireFeedEntry = CreateLocalQuestionnaireFeedEntry();
            newLocalQuestionnaireFeedEntry = CreateLocalQuestionnaireFeedEntry(Guid.NewGuid());
            IEnumerable<AtomFeedEntry<LocalQuestionnaireFeedEntry>> localQuestionnaireFeedEntres = new[]
            { CreateAtomFeedEntry(emptyLocalQuestionnaireFeedEntry), CreateAtomFeedEntry(newLocalQuestionnaireFeedEntry) };

            var atomFeedReaderMock=new Mock<IAtomFeedReader>();

            atomFeedReaderMock.Setup(x => x.ReadAfterAsync<LocalQuestionnaireFeedEntry>(Moq.It.IsAny<Uri>(), Moq.It.IsAny<string>()))
                .Returns(Task.FromResult(localQuestionnaireFeedEntres));

            plainStorageMock=new Mock<IQueryablePlainStorageAccessor<LocalQuestionnaireFeedEntry>>();

            headquartersQuestionnaireReaderMock=new Mock<IHeadquartersQuestionnaireReader>();
            headquartersQuestionnaireReaderMock.Setup(x => x.GetQuestionnaireByUri(Moq.It.IsAny<Uri>()))
                .Returns(Task.FromResult(new QuestionnaireDocument()));

            questionnaireSynchronizer = CreateQuestionnaireSynchronizer(atomFeedReaderMock.Object, plainStorageMock.Object,null, headquartersQuestionnaireReaderMock.Object);
        };

        Because of = () =>
            questionnaireSynchronizer.Pull();

        It should_result = () =>
            plainStorageMock.Verify(x => x.Store(Moq.It.IsAny<LocalQuestionnaireFeedEntry>(),Moq.It.IsAny<string>()), Times.Exactly(2));

        private static QuestionnaireSynchronizer questionnaireSynchronizer;
        private static LocalQuestionnaireFeedEntry emptyLocalQuestionnaireFeedEntry;
        private static LocalQuestionnaireFeedEntry newLocalQuestionnaireFeedEntry;
        private static Mock<IQueryablePlainStorageAccessor<LocalQuestionnaireFeedEntry>> plainStorageMock;
        private static Mock<IHeadquartersQuestionnaireReader> headquartersQuestionnaireReaderMock;
    }
}
