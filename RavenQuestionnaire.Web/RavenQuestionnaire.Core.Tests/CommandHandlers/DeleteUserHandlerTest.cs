using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.CommandHandlers;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.Tests.CommandHandlers
{
    [TestFixture]
    public class DeleteUserHandlerTest
    {
        [Test]
        public void WhenCommandIsReceived_QuestionnaireIsDeletedFromRepository()
        {
            UserDocument innerDocument = new UserDocument();
            innerDocument.Id = "uID";

            User entity = new User(innerDocument);

            Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock.Setup(x => x.Load("userdocuments/uID")).Returns(entity);

            DeleteUserHandler handler = new DeleteUserHandler(userRepositoryMock.Object);
            handler.Handle(new Commands.DeleteUserCommand(entity.UserId));
            userRepositoryMock.Verify(x => x.Remove(entity), Times.Never());
            Assert.IsTrue(innerDocument.IsDeleted);
        }
    }
}
