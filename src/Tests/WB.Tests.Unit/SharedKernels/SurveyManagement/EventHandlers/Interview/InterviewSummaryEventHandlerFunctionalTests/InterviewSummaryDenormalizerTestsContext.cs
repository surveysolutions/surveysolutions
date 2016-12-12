using System;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    [Subject(typeof(InterviewSummaryDenormalizer))]
    public class InterviewSummaryDenormalizerTestsContext
    {
        public static InterviewSummaryDenormalizer CreateDenormalizer()
        {
            return CreateDenormalizer(users: Mock.Of<IPlainStorageAccessor<UserDocument>>());
        }

        public static InterviewSummaryDenormalizer CreateDenormalizer(string userId, string userName)
        {
            return CreateDenormalizer(users: CreateUsersWriterWith1User(userId, userName));
        }

        public static InterviewSummaryDenormalizer CreateDenormalizer(string user1Id, string user1Name,
            string user2Id, string user2Name)
        {
            return CreateDenormalizer(users: CreateUsersWriterWith2Users(user1Id, user1Name, user2Id, user2Name));
        }

        public static InterviewSummaryDenormalizer CreateDenormalizer(IPlainStorageAccessor<UserDocument> users = null)
        {
            var doc = new QuestionnaireDocument();
            return
                new InterviewSummaryDenormalizer(
                    interviewSummary: CreateInterviewSummaryWriter(),
                    users: users ?? CreateUsersWriterWith1User(new Guid().ToString(), new Guid().ToString()),
                    questionnaireStorage:
                        Mock.Of<IQuestionnaireStorage>(
                            _ =>
                                _.GetQuestionnaireDocument(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()) ==
                                doc));
        }

        private static IReadSideRepositoryWriter<InterviewSummary> CreateInterviewSummaryWriter()
        {
            return Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>();
        }

        private static IPlainStorageAccessor<UserDocument> CreateUsersWriterWith1User(string userId, string userName)
        {
            var usersMock = new Mock<IPlainStorageAccessor<UserDocument>>();
            usersMock.Setup(_ => _.GetById(userId)).Returns(new UserDocument() { UserName = userName });
            return usersMock.Object;
        }

        private static IPlainStorageAccessor<UserDocument> CreateUsersWriterWith2Users(string user1Id,
            string user1Name, string user2Id, string user2Name)
        {
            var usersMock = new Mock<IPlainStorageAccessor<UserDocument>>();
            usersMock.Setup(_ => _.GetById(user1Id)).Returns(new UserDocument() {UserName = user1Name});
            usersMock.Setup(_ => _.GetById(user2Id)).Returns(new UserDocument() { UserName = user2Name });

            return usersMock.Object;
        }
    }
}
