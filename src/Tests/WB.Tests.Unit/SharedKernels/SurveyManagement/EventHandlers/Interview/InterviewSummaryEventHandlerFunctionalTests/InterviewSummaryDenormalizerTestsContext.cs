using System;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    [TestOf(typeof(InterviewSummaryDenormalizer))]
    public class InterviewSummaryDenormalizerTestsContext
    {
        public static InterviewSummaryDenormalizer CreateDenormalizer()
        {
            var users = new Mock<IUserViewFactory>();
            users.Setup(x => x.GetUser(Moq.It.IsAny<UserViewInputModel>())).Returns(Create.Entity.UserView());
            return CreateDenormalizer(users.Object);
        }

        public static InterviewSummaryDenormalizer CreateDenormalizer(Guid userId, string userName)
        {
            return CreateDenormalizer(users: CreateUsersWriterWith1User(userId, userName));
        }

        public static InterviewSummaryDenormalizer CreateDenormalizer(Guid user1Id, string user1Name,
            Guid user2Id, string user2Name)
        {
            return CreateDenormalizer(users: CreateUsersWriterWith2Users(user1Id, user1Name, user2Id, user2Name));
        }

        public static InterviewSummaryDenormalizer CreateDenormalizer(IUserViewFactory users = null, IQuestionnaire questionnaire = null)
        {
            var doc = questionnaire ?? Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocument());
            return
                new InterviewSummaryDenormalizer(
                    users: users ?? CreateUsersWriterWith1User(new Guid(), new Guid().ToString()),
                    questionnaireStorage:
                        Mock.Of<IQuestionnaireStorage>(
                            _ =>
                                _.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>()) ==
                                doc),
                    memoryCache: Create.Storage.NewMemoryCache());
        }


        private static IUserViewFactory CreateUsersWriterWith1User(Guid userId, string userName)
        {
            var usersMock = new Mock<IUserViewFactory>();
            usersMock.Setup(_ => _.GetUser(Moq.It.Is<UserViewInputModel>(x=>x.PublicKey == userId))).Returns(new UserView { UserName = userName });
            return usersMock.Object;
        }

        private static IUserViewFactory CreateUsersWriterWith2Users(Guid user1Id,
            string user1Name, Guid user2Id, string user2Name)
        {
            var usersMock = new Mock<IUserViewFactory>();
            usersMock.Setup(_ => _.GetUser(Moq.It.Is<UserViewInputModel>(x=>x.PublicKey ==user1Id))).Returns(new UserView() {UserName = user1Name});
            usersMock.Setup(_ => _.GetUser(Moq.It.Is<UserViewInputModel>(x => x.PublicKey == user2Id))).Returns(new UserView() { UserName = user2Name });

            return usersMock.Object;
        }
    }
}
