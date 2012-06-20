using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.CommandHandlers;
using RavenQuestionnaire.Core.CommandHandlers.Questionnaire.Completed;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question;
using RavenQuestionnaire.Core.Entities.Subscribers;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Services;
using RavenQuestionnaire.Core.Views.Answer;

namespace RavenQuestionnaire.Core.Tests.CommandHandlers
{
    [TestFixture]
    public class UpdateAnswerInCompleteQuestionnaireHandlerTest
    {
       // public Mock<IIteratorContainer> iteratorContainerMock;
        [SetUp]
        public void CreateObjects()
        {
          //  iteratorContainerMock = new Mock<IIteratorContainer>();
        }
        [Test]
        public void WhenCommandIsReceived_NewAnswerIsAddedTCompleteQuestionnaire()
        {
            Mock<ICompleteQuestionnaireRepository> repositoryMock = new Mock<ICompleteQuestionnaireRepository>();
            Mock<IStatisticRepository> statisticMock = new Mock<IStatisticRepository>();
            CompleteQuestionnaireDocument qDoqument= new CompleteQuestionnaireDocument();
            var question = new SingleCompleteQuestion("q");
            
            CompleteAnswer answer = new CompleteAnswer(new Answer());
            question.Children.Add(answer);
            qDoqument.Children.Add(question);
            CompleteQuestionnaire questionanire = new CompleteQuestionnaire(qDoqument);
            repositoryMock.Setup(x => x.Load("cqId")).Returns(questionanire);
            Mock<ISubscriber> subscriberMock = new Mock<ISubscriber>();
            CompleteQuestionnaireUploaderService service = new CompleteQuestionnaireUploaderService(repositoryMock.Object, statisticMock.Object,  subscriberMock.Object);
            UpdateAnswerInCompleteQuestionnaireCommand command = new UpdateAnswerInCompleteQuestionnaireCommand("cqId", question.PublicKey, null, answer.PublicKey, null,null);

            service.AddCompleteAnswer(command.CompleteQuestionnaireId, command.QuestionPublickey,command.Propagationkey,command.CompleteAnswer);
            repositoryMock.Verify(x => x.Load("cqId"), Times.Once());
            Assert.AreEqual(qDoqument.Children[0].PublicKey, question.PublicKey);
            Assert.AreEqual(((ICompleteAnswer)(qDoqument.Children[0].Children[0])).Selected, true);
        }
        [Test]
        public void 
            AddNewAnswerInPropagatedGroup_ValidAnswer_AnswerIsAdded()
        {
            CompleteQuestionnaireDocument qDoqument = new CompleteQuestionnaireDocument();
            CompleteQuestionnaire questionanire = new CompleteQuestionnaire(qDoqument);
            CompleteGroup group = new CompleteGroup("test") { Propagated = Propagate.Propagated};
            var question = new SingleCompleteQuestion("q");
            CompleteAnswer answer = new CompleteAnswer(new Answer());
            question.Children.Add(answer);
            group.Children.Add(question);
            qDoqument.Children.Add(group);
            questionanire.Add(group, null);
            Mock<ICompleteQuestionnaireRepository> repositoryMock = new Mock<ICompleteQuestionnaireRepository>();
            repositoryMock.Setup(x => x.Load("cqId")).Returns(questionanire);

            Mock<IStatisticRepository> statisticMock = new Mock<IStatisticRepository>();

            CompleteAnswer completeAnswer = new CompleteAnswer(answer);
            completeAnswer.Selected = true;
            Mock<ISubscriber> subscriberMock = new Mock<ISubscriber>();
            CompleteQuestionnaireUploaderService handler = new CompleteQuestionnaireUploaderService(repositoryMock.Object, statisticMock.Object, subscriberMock.Object);
            UpdateAnswerInCompleteQuestionnaireCommand command = new UpdateAnswerInCompleteQuestionnaireCommand("cqId", question.PublicKey, ((CompleteGroup)qDoqument.Children[1]).PropogationPublicKey, completeAnswer.PublicKey,null, null);
            handler.AddCompleteAnswer(command.CompleteQuestionnaireId, command.QuestionPublickey, command.Propagationkey, command.CompleteAnswer);

            Assert.AreEqual(((ICompleteAnswer)((qDoqument.Children[0] as CompleteGroup).Children[0] as AbstractCompleteQuestion).Children[0]).Selected, false);
            Assert.AreEqual(((ICompleteAnswer)((qDoqument.Children[1] as CompleteGroup).Children[0] as AbstractCompleteQuestion).Children[0]).Selected, true);
            //  group.Add(group, null);
        }

        [Test]
        public void AddNewAnswerInPropagatedGroup_InvalidPropogationGuid_NoAnswersIsSelected()
        {
            CompleteQuestionnaireDocument qDoqument = new CompleteQuestionnaireDocument();
            CompleteQuestionnaire questionanire = new CompleteQuestionnaire(qDoqument);
            CompleteGroup group = new CompleteGroup("test") { Propagated = Propagate.Propagated };
            var question = new SingleCompleteQuestion("q");
            CompleteAnswer answer = new CompleteAnswer(new Answer());
            question.Children.Add(answer);
            group.Children.Add(question);
            qDoqument.Children.Add(group);
            questionanire.Add(group, null);
            Mock<ICompleteQuestionnaireRepository> repositoryMock = new Mock<ICompleteQuestionnaireRepository>();
            Mock<IStatisticRepository> statisticMock = new Mock<IStatisticRepository>();
            repositoryMock.Setup(x => x.Load("cqId")).Returns(questionanire);

            CompleteAnswer completeAnswer = new CompleteAnswer(answer){ Selected = true };
            Mock<ISubscriber> subscriberMock = new Mock<ISubscriber>();
            CompleteQuestionnaireUploaderService service = new CompleteQuestionnaireUploaderService(repositoryMock.Object, statisticMock.Object,  subscriberMock.Object);
            UpdateAnswerInCompleteQuestionnaireCommand command = new UpdateAnswerInCompleteQuestionnaireCommand("cqId", Guid.NewGuid(), Guid.NewGuid(), completeAnswer.PublicKey,null,null);


            Assert.Throws<ArgumentException>(
                () => service.AddCompleteAnswer(command.CompleteQuestionnaireId, command.QuestionPublickey, command.Propagationkey, command.CompleteAnswers));
            //  group.Add(group, null);fnk
        }
    }
}
