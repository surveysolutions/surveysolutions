using System;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    [Subject(typeof(InterviewSummaryDenormalizer))]
    public class InterviewSummaryDenormalizerTestsContext
    {
        public static InterviewSummaryDenormalizer CreateDenormalizer()
        {
            return CreateDenormalizer(users: Mock.Of<IReadSideRepositoryWriter<UserDocument>>());
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

        public static InterviewSummaryDenormalizer CreateDenormalizer(IReadSideRepositoryWriter<UserDocument> users = null)
        {
            return
                new InterviewSummaryDenormalizer(
                    interviewSummary: CreateInterviewSummaryWriter(),
                    users: users ?? CreateUsersWriterWith1User(new Guid().ToString(), new Guid().ToString()),
                    plainQuestionnaireRepository:
                        Mock.Of<IPlainQuestionnaireRepository>(
                            _ =>
                                _.GetQuestionnaireDocument(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()) ==
                                new QuestionnaireDocument()));
        }

        private static IReadSideRepositoryWriter<InterviewSummary> CreateInterviewSummaryWriter()
        {
            return Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>();
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
