using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Supervisor.Questionnaires;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Services.DeleteQuestionnaireTemplate;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Synchronization.QuestionnaireSynchronizerTests
{
    internal class when_pull_delete_questionnaire_entity_and_interviews_created_by_this_template_are_present : QuestionnaireSynchronizerTestContext
    {
        Establish context = () =>
        {
            deleteLocalQuestionnaireFeedEntry = CreateLocalQuestionnaireFeedEntry(Guid.NewGuid(), QuestionnaireEntryType.QuestionnaireDeleted,
                Guid.NewGuid(), 1);

            IEnumerable<LocalQuestionnaireFeedEntry> localQuestionnaireFeedEntres = new[]
            {
                deleteLocalQuestionnaireFeedEntry
            };


            plainStorageMock = new Mock<IPlainStorageAccessor<LocalQuestionnaireFeedEntry>>();
            plainStorageMock.Setup(
                x => x.Query(Moq.It.IsAny<Func<IQueryable<LocalQuestionnaireFeedEntry>, List<LocalQuestionnaireFeedEntry>>>()))
                .Returns(localQuestionnaireFeedEntres.ToList());

            headquartersQuestionnaireReaderMock = new Mock<IHeadquartersQuestionnaireReader>();
            headquartersQuestionnaireReaderMock.Setup(x => x.GetQuestionnaireByUri(Moq.It.IsAny<Uri>()))
                .Returns(Task.FromResult(new QuestionnaireDocument()));


            plainQuestionnaireRepositoryMock = new Mock<IPlainQuestionnaireRepository>();

            headquartersPullContext = new HeadquartersPullContextStub();

            deleteQuestionnaireServiceMock = new Mock<IDeleteQuestionnaireService>();

            commandServiceMock=new Mock<ICommandService>();
            questionnaireSynchronizer = CreateQuestionnaireSynchronizer(plainStorage: plainStorageMock.Object,
                plainQuestionnaireRepository: plainQuestionnaireRepositoryMock.Object,
                headquartersQuestionnaireReader: headquartersQuestionnaireReaderMock.Object, headquartersPullContext: headquartersPullContext, 
                deleteQuestionnaireService: deleteQuestionnaireServiceMock.Object, commandService: commandServiceMock.Object);
        };

        Because of = () =>
            questionnaireSynchronizer.Pull();

        It should_1_questionnaire_be_deleted_via_delete_questionnaire_service = () =>
             deleteQuestionnaireServiceMock.Verify(x => x.DeleteQuestionnaire(deleteLocalQuestionnaireFeedEntry.QuestionnaireId, deleteLocalQuestionnaireFeedEntry.QuestionnaireVersion, null), Times.Once);


        private static Mock<IDeleteQuestionnaireService> deleteQuestionnaireServiceMock;
        private static QuestionnaireSynchronizer questionnaireSynchronizer;
        private static LocalQuestionnaireFeedEntry deleteLocalQuestionnaireFeedEntry;
        private static Mock<IPlainStorageAccessor<LocalQuestionnaireFeedEntry>> plainStorageMock;
        private static Mock<IHeadquartersQuestionnaireReader> headquartersQuestionnaireReaderMock;
        private static Mock<IPlainQuestionnaireRepository> plainQuestionnaireRepositoryMock;
        private static Mock<ICommandService> commandServiceMock;
        private static HeadquartersPullContextStub headquartersPullContext;

        private static Guid censusModeInterviewId=Guid.NewGuid();
        private static Guid hqCreatedInterviewId = Guid.NewGuid();
    }
}
