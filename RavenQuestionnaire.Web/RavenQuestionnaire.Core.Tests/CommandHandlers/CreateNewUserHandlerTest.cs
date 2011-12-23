using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.CommandHandlers;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.Tests.CommandHandlers
{
    [TestFixture]
    public class CreateNewUserHandlerTest
    {
        [Test]
        public void WhenCommandIsReceived_NewUserIsAddedToRepository()
        {
            Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
            Location location = new Location("test");
            Mock<ILocationRepository> locationRepositoryMock = new Mock<ILocationRepository>();
            locationRepositoryMock.Setup(x => x.Load("locationdocuments/some_id")).Returns(location);
        
            CreateNewUserHandler handler = new CreateNewUserHandler(userRepositoryMock.Object,
                                                                    locationRepositoryMock.Object);
            handler.Handle(new Commands.CreateNewUserCommand("test_user","email@test.com","pass",UserRoles.User, false, null, "some_id", null));
            userRepositoryMock.Verify(x => x.Add(It.IsAny<User>()), Times.Once());
        }

        [Test]
        public void WhenCommandIsReceived_CreateNewUserWithSupervisor_NewUserIsAddedToRepository()
        {
            Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
            UserDocument supervisorDoc= new UserDocument();
            supervisorDoc.UserName = "supervisor";
            supervisorDoc.Password = "1234";
            supervisorDoc.Roles.Add(UserRoles.Supervisor);
            supervisorDoc.IsLocked = false;
            supervisorDoc.Id = "userdocuments/supervisor_id";
            User supervisor = new User(supervisorDoc);
            userRepositoryMock.Setup(x => x.Load("userdocuments/supervisor_id")).Returns(supervisor);
            Location location = new Location("test");
            Mock<ILocationRepository> locationRepositoryMock = new Mock<ILocationRepository>();
            locationRepositoryMock.Setup(x => x.Load("locationdocuments/some_id")).Returns(location);

            CreateNewUserHandler handler = new CreateNewUserHandler(userRepositoryMock.Object,
                                                                    locationRepositoryMock.Object);
            handler.Handle(new Commands.CreateNewUserCommand("test_user", "email@test.com", "pass", UserRoles.User,
                                                             false, "supervisor_id", "some_id", null));
            userRepositoryMock.Verify(
                x =>
                x.Add(
                    It.Is<User>(
                        u =>
                        ((IEntity<UserDocument>) u).GetInnerDocument().Supervisor.Id ==
                        "userdocuments/supervisor_id")), Times.Once());
            userRepositoryMock.Verify(x => x.Load("userdocuments/supervisor_id"));
        }
    }
}
