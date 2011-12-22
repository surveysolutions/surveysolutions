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
using RavenQuestionnaire.Core.Services;

namespace RavenQuestionnaire.Core.Tests.CommandHandlers
{
    [TestFixture]
    public class UpdateCompleteQuestionnaireHandlerTest
    {
        [Test]
        public void WhenCommandIsReceived_CompleteQuestionnaireIsUpdatedToRepository()
        {
            CompleteQuestionnaireDocument innerDocument = new CompleteQuestionnaireDocument();
            innerDocument.Id = "cqID";
            innerDocument.Questionnaire = new QuestionnaireDocument() { Id = "qID" };
            CompleteQuestionnaire entity = new CompleteQuestionnaire(innerDocument);
            Questionnaire qEntity= new Questionnaire(innerDocument.Questionnaire);
            Mock<ICompleteQuestionnaireRepository> coompleteQuestionnaireRepositoryMock = new Mock<ICompleteQuestionnaireRepository>();

            Mock<IStatusRepository> statusRepositoryMock = new Mock<IStatusRepository>();

            coompleteQuestionnaireRepositoryMock.Setup(x => x.Load("completequestionnairedocuments/cqID")).Returns(entity);

            UpdateCompleteQuestionnaireHandler handler = new UpdateCompleteQuestionnaireHandler(coompleteQuestionnaireRepositoryMock.Object, 
                statusRepositoryMock.Object);

            handler.Handle(new Commands.UpdateCompleteQuestionnaireCommand("cqID",  new CompleteAnswer[0], "-11"));

            coompleteQuestionnaireRepositoryMock.Verify(x => x.Load("completequestionnairedocuments/cqID"));

        }
    }
}
