using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.CommandHandlers;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.Tests.CommandHandlers
{
    [TestFixture]
    public class CreateNewLocationHandlerTest
    {
         [Test]
         public void WhenCommandIsReceived_NewLocationIsAddedToRepository()
         {
        //     Mock<ILocationRepository> userRepositoryMock = new Mock<ILocationRepository>();
             Location location = new Location("test");
             Mock<ILocationRepository> locationRepositoryMock = new Mock<ILocationRepository>();
          //   locationRepositoryMock.Setup(x => x.Load("locationdocuments/some_id")).Returns(location);

             CreateNewLocationHandler handler = new CreateNewLocationHandler(locationRepositoryMock.Object);
             handler.Handle(new Commands.CreateNewLocationCommand("some location", null));
             locationRepositoryMock.Verify(x => x.Add(It.IsAny<Location>()), Times.Once());
         }
    }
}
