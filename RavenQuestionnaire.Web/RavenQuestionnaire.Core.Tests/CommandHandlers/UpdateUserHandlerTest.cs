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
                                                          new [] {UserRoles.Administrator}, null,"some_id"));

            Assert.True(
                innerDocument.Email == "email@test.com" /*&& innerDocument.Password == "test"*/ && innerDocument.IsLocked &&
                innerDocument.Roles.Count == 1 && innerDocument.Roles.Contains(UserRoles.Administrator)); 

        }
    }
}
