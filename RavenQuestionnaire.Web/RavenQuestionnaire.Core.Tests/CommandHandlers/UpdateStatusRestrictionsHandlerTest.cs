using System;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Commands.Status;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.CommandHandlers.Status;


namespace RavenQuestionnaire.Core.Tests.CommandHandlers
{
    [TestFixture]
    public class UpdateStatusRestrictionsHandlerTest
    {
        [Test]
        public void WhenCommandIsReceived_QuestionIsUpdatedToRepository()
        {
            StatusDocument innerDocument = new StatusDocument();
            innerDocument.Id = "uID";
            Guid publicKey = Guid.NewGuid();

            innerDocument.Statuses.Add(new StatusItem(){PublicKey = publicKey, Title = "tesstt"});

            Status entity = new Status(innerDocument);
            Mock<IStatusRepository> statusRepositoryMock = new Mock<IStatusRepository>();
            statusRepositoryMock.Setup(x => x.Load("statusdocuments/uID")).Returns(entity);
            UpdateStatusRestrictionsHandler handler = new UpdateStatusRestrictionsHandler(statusRepositoryMock.Object);
            Dictionary<string, List<SurveyStatus>> test = new Dictionary<string, List<SurveyStatus>>();
            List<SurveyStatus> statuses = new List<SurveyStatus>() { new SurveyStatus() { PublicId = Guid.NewGuid(),Name = "someName" } };
            test.Add("Manager", statuses);


            handler.Handle(new UpdateStatusRestrictionsCommand("Qid", "uID", publicKey, test, null));

            Assert.True(innerDocument.Statuses[0].IsVisible && innerDocument.Statuses[0].StatusRoles.Count == 1);
        }
    }
}
