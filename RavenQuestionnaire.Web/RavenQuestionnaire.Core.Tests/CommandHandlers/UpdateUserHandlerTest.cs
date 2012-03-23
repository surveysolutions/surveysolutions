using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.CommandHandlers;
using RavenQuestionnaire.Core.CommandHandlers.User;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.Tests.CommandHandlers
{
    [TestFixture]
    public class UpdateUserHandlerTest
    {
        [Test]
        public void WhenCommandIsReceived_QuestionIsUpdatedToRepository()
        {
            UserDocument innerDocument = new UserDocument();
            innerDocument.Id = "uID";
            User entity = new User(innerDocument);
            Location location= new Location("test");
            Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
            Mock<ILocationRepository> locationRepositoryMock = new Mock<ILocationRepository>();
            userRepositoryMock.Setup(x => x.Load("userdocuments/uID")).Returns(entity);
            locationRepositoryMock.Setup(x => x.Load("locationdocuments/some_id")).Returns(location);
            UpdateUserHandler handler = new UpdateUserHandler(userRepositoryMock.Object, locationRepositoryMock.Object);
            handler.Handle(new Commands.UpdateUserCommand("uID", "email@test.com",/* "test",*/ true,
                                                          new[] { UserRoles.Administrator }, null, 
                                                          "some_id", null));

            Assert.True(
                innerDocument.Email == "email@test.com" /*&& innerDocument.Password == "test"*/ && innerDocument.IsLocked &&
                innerDocument.Roles.Count == 1 && innerDocument.Roles.Contains(UserRoles.Administrator)); 

        }

        [Test]
        public void WhenCommandIsReceived_UserUserWithSupervisor_UserIsUpdated()
        {
            Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();

            UserDocument innerDocument = new UserDocument();
            innerDocument.Id = "userdocuments/uID";
            User entity = new User(innerDocument);

            UserDocument supervisorDoc = new UserDocument();
            supervisorDoc.UserName = "supervisor";
            supervisorDoc.Password = "1234";
            supervisorDoc.Roles.Add(UserRoles.Supervisor);
            supervisorDoc.IsLocked = false;
            supervisorDoc.Id = "userdocuments/supervisor_id";
            User supervisor = new User(supervisorDoc);
            userRepositoryMock.Setup(x => x.Load("userdocuments/supervisor_id")).Returns(supervisor);
            userRepositoryMock.Setup(x => x.Load("userdocuments/uID")).Returns(entity);
            Location location = new Location("test");
            Mock<ILocationRepository> locationRepositoryMock = new Mock<ILocationRepository>();
            locationRepositoryMock.Setup(x => x.Load("locationdocuments/some_id")).Returns(location);

            UpdateUserHandler handler = new UpdateUserHandler(userRepositoryMock.Object,
                                                                    locationRepositoryMock.Object);
            handler.Handle(new Commands.UpdateUserCommand("uID", "email@test.com", false,
                                                          new UserRoles[] {UserRoles.User},
                                                          "supervisor_id", "some_id", null));
            Assert.AreEqual(innerDocument.Supervisor.Id, supervisorDoc.Id);
            Assert.AreEqual(innerDocument.Supervisor.Name, supervisorDoc.UserName);
            userRepositoryMock.Verify(x => x.Load("userdocuments/supervisor_id"));
        }
    }
}
