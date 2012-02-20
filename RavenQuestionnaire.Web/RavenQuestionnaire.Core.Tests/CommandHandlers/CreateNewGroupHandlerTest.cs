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
    public class CreateNewGroupHandlerTest
    {
        [Test]
        public void WhenCommandIsReceived_TopLevelGroup_NewGroupIsIsAddedToQuestionnaire()
        {
            Mock<IQuestionnaireRepository> questionaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            Questionnaire questionnaire= new Questionnaire("some questionanire");
            questionaireRepositoryMock.Setup(x => x.Load("questionnairedocuments/qId")).Returns(questionnaire);
            CreateNewGroupHandler handler = new CreateNewGroupHandler(questionaireRepositoryMock.Object);
            handler.Handle(new Commands.CreateNewGroupCommand("some text", Propagate.None, "qId", null, null));
            var innerDocument = ((IEntity<QuestionnaireDocument>) questionnaire).GetInnerDocument();
            Assert.AreEqual(innerDocument.Groups.Count, 1);
            Assert.AreEqual(innerDocument.Groups[0].Title, "some text");
        }
        [Test]
        public void WhenCommandIsReceived_SubGroup_NewGroupIsIsAddedToQuestionnaire()
        {
            Mock<IQuestionnaireRepository> questionaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            Questionnaire questionnaire = new Questionnaire("some questionanire");
            var innerDocument = ((IEntity<QuestionnaireDocument>)questionnaire).GetInnerDocument();
            Group topGroup = new Group("top group");
            innerDocument.Groups.Add(topGroup);
            questionaireRepositoryMock.Setup(x => x.Load("questionnairedocuments/qId")).Returns(questionnaire);
            CreateNewGroupHandler handler = new CreateNewGroupHandler(questionaireRepositoryMock.Object);
            handler.Handle(new Commands.CreateNewGroupCommand("some text", Propagate.None, "qId", topGroup.PublicKey, null));
          
            Assert.AreEqual((innerDocument.Groups[0] as Group).Groups.Count, 1);
            Assert.AreEqual((innerDocument.Groups[0] as Group).Groups[0].Title, "some text");
        }

        [Test]
        public void WhenCommandIsReceived_ParentGroupNotExists_NewGroupIsIsAddedToQuestionnaire()
        {
            Mock<IQuestionnaireRepository> questionaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            Questionnaire questionnaire = new Questionnaire("some questionanire");
            questionaireRepositoryMock.Setup(x => x.Load("questionnairedocuments/qId")).Returns(questionnaire);
            CreateNewGroupHandler handler = new CreateNewGroupHandler(questionaireRepositoryMock.Object);

            Assert.Throws<ArgumentException>(
                () => handler.Handle(new Commands.CreateNewGroupCommand("some text", Propagate.None, "qId", Guid.NewGuid(), null)));
        }
    }
}
