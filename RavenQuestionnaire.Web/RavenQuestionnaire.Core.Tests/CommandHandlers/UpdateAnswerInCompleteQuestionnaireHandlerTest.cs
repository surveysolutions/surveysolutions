using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.CommandHandlers;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Views.Answer;

namespace RavenQuestionnaire.Core.Tests.CommandHandlers
{
    [TestFixture]
    public class UpdateAnswerInCompleteQuestionnaireHandlerTest
    {
        public Mock<IIteratorContainer> iteratorContainerMock;
        [SetUp]
        public void CreateObjects()
        {
            iteratorContainerMock = new Mock<IIteratorContainer>();
        }
        [Test]
        public void WhenCommandIsReceived_NewAnswerIsAddedTCompleteQuestionnaire()
        {
            Mock<ICompleteQuestionnaireRepository> repositoryMock = new Mock<ICompleteQuestionnaireRepository>();
            CompleteQuestionnaireDocument qDoqument= new CompleteQuestionnaireDocument();
            CompleteQuestion question = new CompleteQuestion("q",
                                             QuestionType.SingleOption);
            
            CompleteAnswer answer = new CompleteAnswer(new Answer(), Guid.NewGuid());
            question.Answers.Add(answer);
            qDoqument.Questions.Add(question);
            CompleteQuestionnaire questionanire = new CompleteQuestionnaire(qDoqument, iteratorContainerMock.Object);
            repositoryMock.Setup(x => x.Load("completequestionnairedocuments/cqId")).Returns(questionanire);
            UpdateAnswerInCompleteQuestionnaireHandler handler = new UpdateAnswerInCompleteQuestionnaireHandler(repositoryMock.Object, new CompleteQuestionnaireConditionExecutor());
            UpdateAnswerInCompleteQuestionnaireCommand command = new UpdateAnswerInCompleteQuestionnaireCommand("cqId",new CompleteAnswerView[]{
                                                                                                                new CompleteAnswerView
                                                                                                                    (
                                                                                                                    question
                                                                                                                        .
                                                                                                                        PublicKey,
                                                                                                                    answer){ Selected = true}},
                                                                                                                null
                                                                                                                ,
                                                                                                                null);
            iteratorContainerMock.Setup(
             x => x.Create<ICompleteGroup, ICompleteQuestion>(qDoqument)).Returns(
                 new QuestionnaireQuestionIterator(qDoqument));
            handler.Handle(command);
            repositoryMock.Verify(x => x.Load("completequestionnairedocuments/cqId"), Times.Once());
            Assert.AreEqual(qDoqument.Questions[0].PublicKey, question.PublicKey);
            Assert.AreEqual((qDoqument.Questions[0] as CompleteQuestion).Answers[0].Selected, true);
        }
        [Test]
        public void 
            AddNewAnswerInPropagatedGroup_ValidAnswer_AnswerIsAdded()
        {
            CompleteQuestionnaireDocument qDoqument = new CompleteQuestionnaireDocument();
            CompleteQuestionnaire questionanire = new CompleteQuestionnaire(qDoqument, iteratorContainerMock.Object);
            CompleteGroup group = new CompleteGroup("test") { Propagated = Propagate.Propagated};
            CompleteQuestion question = new CompleteQuestion("q",
                                           QuestionType.SingleOption);
            CompleteAnswer answer = new CompleteAnswer(new Answer(), Guid.NewGuid());
            question.Answers.Add(answer);
            group.Questions.Add(question);
            qDoqument.Groups.Add(group);
            questionanire.Add(group, null);
            Mock<ICompleteQuestionnaireRepository> repositoryMock = new Mock<ICompleteQuestionnaireRepository>();
            repositoryMock.Setup(x => x.Load("completequestionnairedocuments/cqId")).Returns(questionanire);

            CompleteAnswer completeAnswer = new CompleteAnswer(answer, question.PublicKey);
            completeAnswer.Selected = true;
            iteratorContainerMock.Setup(
               x => x.Create<CompleteQuestionnaireDocument, ICompleteQuestion>(qDoqument)).Returns(
                   new QuestionnaireQuestionIterator(qDoqument));
            UpdateAnswerInCompleteQuestionnaireHandler handler = new UpdateAnswerInCompleteQuestionnaireHandler(repositoryMock.Object, new CompleteQuestionnaireConditionExecutor());
            UpdateAnswerInCompleteQuestionnaireCommand command = new UpdateAnswerInCompleteQuestionnaireCommand("cqId",
                new CompleteAnswerView[]{
                                                                                                                new CompleteAnswerView
                                                                                                                    (completeAnswer)},
                                                                                                                ((
                                                                                                                 PropagatableCompleteGroup
                                                                                                                 )
                                                                                                                 qDoqument
                                                                                                                     .
                                                                                                                     Groups
                                                                                                                     [
                                                                                                                         1
                                                                                                                     ])
                                                                                                                    .
                                                                                                                    PropogationPublicKey
                                                                                                                ,
                                                                                                                null);
            handler.Handle(command);

            Assert.AreEqual(((qDoqument.Groups[0] as CompleteGroup).Questions[0] as CompleteQuestion).Answers[0].Selected, false);
            Assert.AreEqual(((qDoqument.Groups[1] as CompleteGroup).Questions[0] as CompleteQuestion).Answers[0].Selected, true);
            //  group.Add(group, null);
        }

        [Test]
        public void AddNewAnswerInPropagatedGroup_InvalidPropogationGuid_NoAnswersIsSelected()
        {
            CompleteQuestionnaireDocument qDoqument = new CompleteQuestionnaireDocument();
            CompleteQuestionnaire questionanire = new CompleteQuestionnaire(qDoqument, iteratorContainerMock.Object);
            CompleteGroup group = new CompleteGroup("test") { Propagated = Propagate.Propagated };
            CompleteQuestion question = new CompleteQuestion("q",
                                           QuestionType.SingleOption);
            CompleteAnswer answer = new CompleteAnswer(new Answer(), Guid.NewGuid());
            question.Answers.Add(answer);
            group.Questions.Add(question);
            qDoqument.Groups.Add(group);
            questionanire.Add(group, null);
            Mock<ICompleteQuestionnaireRepository> repositoryMock = new Mock<ICompleteQuestionnaireRepository>();
            repositoryMock.Setup(x => x.Load("completequestionnairedocuments/cqId")).Returns(questionanire);

            CompleteAnswer completeAnswer = new CompleteAnswer(answer, question.PublicKey);

            iteratorContainerMock.Setup(
              x => x.Create<ICompleteGroup, ICompleteQuestion>(qDoqument)).Returns(
                  new QuestionnaireQuestionIterator(qDoqument));
            iteratorContainerMock.Setup(
             x => x.Create<ICompleteGroup, ICompleteAnswer>(qDoqument)).Returns(
                 new QuestionnaireAnswerIterator(qDoqument));
            UpdateAnswerInCompleteQuestionnaireHandler handler = new UpdateAnswerInCompleteQuestionnaireHandler(repositoryMock.Object, new CompleteQuestionnaireConditionExecutor());
            UpdateAnswerInCompleteQuestionnaireCommand command = new UpdateAnswerInCompleteQuestionnaireCommand("cqId",new CompleteAnswerView[]{
                                                                                                                new CompleteAnswerView
                                                                                                                    (completeAnswer)},
                                                                                                                Guid.
                                                                                                                    NewGuid
                                                                                                                    ()
                                                                                                                ,
                                                                                                                null);


            Assert.Throws<CompositeException>(() => handler.Handle(command));
            //  group.Add(group, null);
        }
    }
}
