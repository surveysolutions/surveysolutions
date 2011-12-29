using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework;

using RavenQuestionnaire.Core.CommandHandlers;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.Tests.CommandHandlers
{
    [TestFixture]
    public class CreateNewQuestionnaireHandlerTest
    {
        [Test]
        public void WhenCommandIsReceived_NewQuestionnaireIsAddedToRepository()
        {
            Mock<IQuestionnaireRepository> questionaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            CreateNewQuestionnaireHandler handler = new CreateNewQuestionnaireHandler(questionaireRepositoryMock.Object);
            handler.Handle(new Commands.CreateNewQuestionnaireCommand("questionnairie", null));

            questionaireRepositoryMock.Verify(x => x.Add(It.IsAny<Questionnaire>()), Times.Once());
        }
    }
}
