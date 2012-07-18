using System;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.CommandHandlers.Questionnaire.Group;
using RavenQuestionnaire.Core.Commands.Questionnaire.Group;
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
            Guid key = Guid.NewGuid();
            Questionnaire questionnaire = new Questionnaire("some questionanire", key);
            questionaireRepositoryMock.Setup(x => x.Load(key.ToString())).Returns(questionnaire);
            CreateNewGroupHandler handler = new CreateNewGroupHandler(questionaireRepositoryMock.Object);
            handler.Handle(new CreateNewGroupCommand("some text", Guid.NewGuid(), Propagate.None, key.ToString(), null, null, string.Empty));
            var innerDocument = ((IEntity<QuestionnaireDocument>) questionnaire).GetInnerDocument();
            Assert.AreEqual(innerDocument.Children.Count, 1);
            Assert.AreEqual(((IGroup)innerDocument.Children[0]).Title, "some text");
        }
        [Test]
        public void WhenCommandIsReceived_SubGroup_NewGroupIsIsAddedToQuestionnaire()
        {
            Mock<IQuestionnaireRepository> questionaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            Guid key = Guid.NewGuid();
            Questionnaire questionnaire = new Questionnaire("some questionanire", key);
            var innerDocument = ((IEntity<QuestionnaireDocument>)questionnaire).GetInnerDocument();
            Group topGroup = new Group("top group");
            innerDocument.Children.Add(topGroup);
            questionaireRepositoryMock.Setup(x => x.Load(key.ToString())).Returns(questionnaire);
            CreateNewGroupHandler handler = new CreateNewGroupHandler(questionaireRepositoryMock.Object);
            handler.Handle(new CreateNewGroupCommand("some text", Guid.NewGuid(), Propagate.None, key.ToString(), topGroup.PublicKey, null, string.Empty));

            Assert.AreEqual((innerDocument.Children[0] as Group).Children.Count, 1);
            Assert.AreEqual(((IGroup)(innerDocument.Children[0] as Group).Children[0]).Title, "some text");
        }

        [Test]
        public void WhenCommandIsReceived_ParentGroupNotExists_NewGroupIsIsAddedToQuestionnaire()
        {
            Mock<IQuestionnaireRepository> questionaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            Guid key = Guid.NewGuid();
            Questionnaire questionnaire = new Questionnaire("some questionanire", key);
            questionaireRepositoryMock.Setup(x => x.Load(key.ToString())).Returns(questionnaire);
            CreateNewGroupHandler handler = new CreateNewGroupHandler(questionaireRepositoryMock.Object);

            Assert.Throws<ArgumentException>(
                () => handler.Handle(new CreateNewGroupCommand("some text", Guid.NewGuid(), Propagate.None, key.ToString(), Guid.NewGuid(), null, string.Empty)));
        }
    }
}
