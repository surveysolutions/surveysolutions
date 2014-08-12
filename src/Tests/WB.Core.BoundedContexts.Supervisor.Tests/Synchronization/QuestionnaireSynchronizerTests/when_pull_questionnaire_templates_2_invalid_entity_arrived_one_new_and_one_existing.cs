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
using WB.Core.BoundedContexts.Supervisor.Synchronization;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Atom;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Synchronization.QuestionnaireSynchronizerTests
{
    internal class when_pull_questionnaire_templates_2_invalid_entity_arrived_one_new_and_one_existing : QuestionnaireSynchronizerTestContext
    {
        Establish context = () =>
        {
            emptyLocalQuestionnaireFeedEntry = CreateLocalQuestionnaireFeedEntry();
            withALotOfExceptionsLocalQuestionnaireFeedEntry = CreateLocalQuestionnaireFeedEntry();
            newLocalQuestionnaireFeedEntry = CreateLocalQuestionnaireFeedEntry(Guid.NewGuid());
            existingLocalQuestionnaireFeedEntry = CreateLocalQuestionnaireFeedEntry(Guid.NewGuid());
            deleteLocalQuestionnaireFeedEntry = CreateLocalQuestionnaireFeedEntry(Guid.NewGuid(), QuestionnaireEntryType.DeleteQuestionnaire,
                Guid.NewGuid(), 1);

            IEnumerable<AtomFeedEntry<LocalQuestionnaireFeedEntry>> localQuestionnaireFeedEntres = new[]
            {
                CreateAtomFeedEntry(withALotOfExceptionsLocalQuestionnaireFeedEntry), 
                CreateAtomFeedEntry(emptyLocalQuestionnaireFeedEntry), 
                CreateAtomFeedEntry(newLocalQuestionnaireFeedEntry),
                CreateAtomFeedEntry(existingLocalQuestionnaireFeedEntry),
                CreateAtomFeedEntry(deleteLocalQuestionnaireFeedEntry)
            };

            var atomFeedReaderMock=new Mock<IAtomFeedReader>();

            atomFeedReaderMock.Setup(x => x.ReadAfterAsync<LocalQuestionnaireFeedEntry>(Moq.It.IsAny<Uri>(), Moq.It.IsAny<string>()))
                .Returns(Task.FromResult(localQuestionnaireFeedEntres));

            plainStorageMock=new Mock<IQueryablePlainStorageAccessor<LocalQuestionnaireFeedEntry>>();

            headquartersQuestionnaireReaderMock=new Mock<IHeadquartersQuestionnaireReader>();
            headquartersQuestionnaireReaderMock.Setup(x => x.GetQuestionnaireByUri(Moq.It.IsAny<Uri>()))
                .Returns(Task.FromResult(new QuestionnaireDocument()));


            plainQuestionnaireRepositoryMock=new Mock<IPlainQuestionnaireRepository>();
            plainQuestionnaireRepositoryMock.Setup(
                x =>
                    x.GetQuestionnaireDocument(emptyLocalQuestionnaireFeedEntry.QuestionnaireId,
                        emptyLocalQuestionnaireFeedEntry.QuestionnaireVersion)).Throws<NullReferenceException>();

            plainQuestionnaireRepositoryMock.Setup(
                x =>
                    x.GetQuestionnaireDocument(withALotOfExceptionsLocalQuestionnaireFeedEntry.QuestionnaireId,
                        withALotOfExceptionsLocalQuestionnaireFeedEntry.QuestionnaireVersion))
                .Throws(new AggregateException(new[] { new NullReferenceException() }));

            plainQuestionnaireRepositoryMock.Setup(
               x =>
                   x.GetQuestionnaireDocument(existingLocalQuestionnaireFeedEntry.QuestionnaireId,
                       existingLocalQuestionnaireFeedEntry.QuestionnaireVersion)).Returns(new QuestionnaireDocument());

            headquartersPullContext=new HeadquartersPullContextTestable();

            questionnaireSynchronizer = CreateQuestionnaireSynchronizer(atomFeedReaderMock.Object, plainStorageMock.Object, plainQuestionnaireRepositoryMock.Object, headquartersQuestionnaireReaderMock.Object, headquartersPullContext);
        };

        Because of = () =>
            questionnaireSynchronizer.Pull();

        It should_in_plain_storage_be_stored_5_entities = () =>
            plainStorageMock.Verify(x => x.Store(Moq.It.IsAny<LocalQuestionnaireFeedEntry>(),Moq.It.IsAny<string>()), Times.Exactly(5));

        It should_in_2_errors_be_pushed_to_HeadquartersPullContext = () =>
            headquartersPullContext.PushedErrorsCount.ShouldEqual(2);

        It should_1_questionnaire_be_stored_in_plain_questionnaire_repository = () =>
            plainQuestionnaireRepositoryMock.Verify(x=>x.StoreQuestionnaire(newLocalQuestionnaireFeedEntry.QuestionnaireId,newLocalQuestionnaireFeedEntry.QuestionnaireVersion, Moq.It.IsAny<QuestionnaireDocument>()), Times.Once);

        It should_1_questionnaire_be_deleted_in_plain_questionnaire_repository = () =>
            plainQuestionnaireRepositoryMock.Verify(x => x.DeleteQuestionnaireDocument(deleteLocalQuestionnaireFeedEntry.QuestionnaireId, deleteLocalQuestionnaireFeedEntry.QuestionnaireVersion), Times.Once);

        private static QuestionnaireSynchronizer questionnaireSynchronizer;
        private static LocalQuestionnaireFeedEntry emptyLocalQuestionnaireFeedEntry;
        private static LocalQuestionnaireFeedEntry withALotOfExceptionsLocalQuestionnaireFeedEntry;
        private static LocalQuestionnaireFeedEntry newLocalQuestionnaireFeedEntry;
        private static LocalQuestionnaireFeedEntry existingLocalQuestionnaireFeedEntry;
        private static LocalQuestionnaireFeedEntry deleteLocalQuestionnaireFeedEntry;
        private static Mock<IQueryablePlainStorageAccessor<LocalQuestionnaireFeedEntry>> plainStorageMock;
        private static Mock<IHeadquartersQuestionnaireReader> headquartersQuestionnaireReaderMock;
        private static Mock<IPlainQuestionnaireRepository> plainQuestionnaireRepositoryMock;
        private static HeadquartersPullContextTestable headquartersPullContext;
    }

    internal class HeadquartersPullContextTestable : HeadquartersPullContext
    {
        private int pushedErrorsCount = 0;
        public int PushedErrorsCount { get { return pushedErrorsCount; } }
        public HeadquartersPullContextTestable()
            : base(Mock.Of<IPlainStorageAccessor<SynchronizationStatus>>()) { }

        public override void PushError(string message)
        {
            base.PushError(message);
            pushedErrorsCount++;
        }
    }
}
