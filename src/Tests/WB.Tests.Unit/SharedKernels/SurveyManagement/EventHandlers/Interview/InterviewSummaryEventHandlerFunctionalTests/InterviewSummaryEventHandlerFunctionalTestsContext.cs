using System;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    [Subject(typeof(InterviewSummaryEventHandlerFunctional))]
    internal class InterviewSummaryEventHandlerFunctionalTestsContext
    {
        public static InterviewSummaryEventHandlerFunctional CreateDenormalizer()
        {
            return CreateDenormalizer(users: Mock.Of<IReadSideRepositoryWriter<UserDocument>>());
        }

        public static InterviewSummaryEventHandlerFunctional CreateDenormalizer(string userId, string userName)
        {
            return CreateDenormalizer(users: CreateUsersWriterWith1User(userId, userName));
        }

        public static InterviewSummaryEventHandlerFunctional CreateDenormalizer(string user1Id, string user1Name,
            string user2Id, string user2Name)
        {
            return CreateDenormalizer(users: CreateUsersWriterWith2Users(user1Id, user1Name, user2Id, user2Name));
        }

        public static InterviewSummaryEventHandlerFunctional CreateDenormalizer(IReadSideRepositoryWriter<UserDocument> users = null)
        {
            return
                new InterviewSummaryEventHandlerFunctional(
                    interviewSummary: CreateInterviewSummaryWriter(), questionnaires: CreateQuestionnaire(),
                    users: users ?? CreateUsersWriterWith1User(new Guid().ToString(), new Guid().ToString()));
        }

        private static IReadSideRepositoryWriter<InterviewSummary> CreateInterviewSummaryWriter()
        {
            return Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>();
        }

        private static IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> CreateQuestionnaire()
        {
            var questionnaireMock = new Mock<IReadSideKeyValueStorage<QuestionnaireDocumentVersioned>>();
            questionnaireMock.Setup(_ => _.GetById(Moq.It.IsAny<string>()))
                .Returns(new QuestionnaireDocumentVersioned() { Questionnaire = new QuestionnaireDocument() });
            return questionnaireMock.Object;
        }

        private static IReadSideRepositoryWriter<UserDocument> CreateUsersWriterWith1User(string userId, string userName)
        {
            var usersMock = new Mock<IReadSideRepositoryWriter<UserDocument>>();
            usersMock.Setup(_ => _.GetById(userId)).Returns(new UserDocument() { UserName = userName });
            return usersMock.Object;
        }

        private static IReadSideRepositoryWriter<UserDocument> CreateUsersWriterWith2Users(string user1Id,
            string user1Name, string user2Id, string user2Name)
        {
            var usersMock = new Mock<IReadSideRepositoryWriter<UserDocument>>();
            usersMock.Setup(_ => _.GetById(user1Id)).Returns(new UserDocument() {UserName = user1Name});
            usersMock.Setup(_ => _.GetById(user2Id)).Returns(new UserDocument() { UserName = user2Name });

            return usersMock.Object;
        }
    }
}
