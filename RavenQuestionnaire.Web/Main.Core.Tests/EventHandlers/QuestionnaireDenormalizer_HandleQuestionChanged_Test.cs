// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireDenormalizer_HandleQuestionChanged_Test.cs" company="">
//   
// </copyright>
// <summary>
//   This is a test class for QuestionnaireDenormalizer_HandleQuestionChanged_Test and is intended
//   to contain all QuestionnaireDenormalizer_HandleQuestionChanged_Test Unit Tests
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.DenormalizerStorage.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.AbstractFactories;
    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Question;
    using Main.Core.EventHandlers;
    using Main.Core.Events.Questionnaire;

    using Moq;

    using Ncqrs.Eventing;
    using Ncqrs.Eventing.ServiceModel.Bus;

    using NUnit.Framework;

    /// <summary>
    /// This is a test class for QuestionnaireDenormalizerTest and is intended
    /// to contain all QuestionnaireDenormalizerTest Unit Tests
    /// </summary>
    [TestFixture]
    public class QuestionnaireDenormalizerHandleQuestionChangedTest
    {
        /// <summary>
        /// Test for updating autopropagate question
        /// </summary>
        [Test]
        public void HandleQuestionChanged_QuestionUpdateEventIsCome_AllAbstractQuestionFieldsAreUpdated()
        {
            var innerDocument = new QuestionnaireDocument();
            var group = new Group("group");
            innerDocument.Children.Add(group);
            var question = new TextQuestion("question")
            {
                QuestionType = QuestionType.Text,
                AnswerOrder = Order.AZ,
                Capital = false,
                ConditionExpression = "[f7b6842d-c17f-495c-bcbd-ba96dd64e527]==1",
                Featured = false,
                Answers = null,
                Comments = "no comments",
                Instructions = string.Empty,
                StataExportCaption = "text",
                ValidationExpression = string.Empty,
                ValidationMessage = string.Empty
            };
            group.Children.Add(question);

            var documentStorage = new Mock<IDenormalizerStorage<QuestionnaireDocument>>();

            documentStorage.Setup(d => d.GetByGuid(innerDocument.PublicKey)).Returns(innerDocument);

            var target = new QuestionnaireDenormalizer(documentStorage.Object, new CompleteQuestionFactory());

            var evnt = new QuestionChanged
            {
                QuestionText = "What is your name",
                QuestionType = QuestionType.Text,
                PublicKey = question.PublicKey,
                Featured = true,
                AnswerOrder = Order.AsIs,
                ConditionExpression = string.Empty,
                Answers = null,
                Instructions = "Answer this question, please",
                StataExportCaption = "name",
                ValidationExpression = "[this]!=''",
                ValidationMessage = "Empty names is invalid answer"
            };

            IPublishedEvent<QuestionChanged> e =
                new PublishedEvent<QuestionChanged>(
                    new UncommittedEvent(
                        Guid.NewGuid(),
                        innerDocument.PublicKey,
                        1,
                        1,
                        DateTime.Now,
                        evnt,
                        new Version(1, 0)));

            target.Handle(e);

            var resultQuestion = group.Find<IQuestion>(question.PublicKey);

            Assert.AreEqual(evnt.QuestionText, resultQuestion.QuestionText, "QuestionText is not updated");
            Assert.AreEqual(evnt.QuestionType, resultQuestion.QuestionType, "QuestionType is not updated");
            Assert.AreEqual(evnt.Featured, resultQuestion.Featured, "Featured is not updated");
            Assert.AreEqual(evnt.AnswerOrder, resultQuestion.AnswerOrder, "AnswerOrder is not updated");
            Assert.AreEqual(evnt.ConditionExpression, resultQuestion.ConditionExpression, "ConditionExpression is not updated");
            Assert.AreEqual(evnt.Instructions, resultQuestion.Instructions, "Instructions is not updated");
            Assert.AreEqual(evnt.StataExportCaption, resultQuestion.StataExportCaption, "StataExportCaption is not updated");
            Assert.AreEqual(evnt.ValidationExpression, resultQuestion.ValidationExpression, "ValidationExpression is not updated");
            Assert.AreEqual(evnt.ValidationMessage, resultQuestion.ValidationMessage, "ValidationMessage is not updated");

        }

        /// <summary>
        /// Test for updating autopropagate question
        /// </summary>
        [Test]
        public void HandleQuestionChanged_AutopropagateQuestionUpdateEventIsCome_TriggersAndMaxValueUpdated()
        {
            var triggers = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var innerDocument = new QuestionnaireDocument();
            var group = new Group("group");
            innerDocument.Children.Add(group);
            var question = new AutoPropagateQuestion("question")
                { 
                    MaxValue = 8, 
                    Triggers = new List<Guid>(), 
                    QuestionType = QuestionType.AutoPropagate 
                };
            group.Children.Add(question);

            var documentStorage = new Mock<IDenormalizerStorage<QuestionnaireDocument>>();

            documentStorage.Setup(d => d.GetByGuid(innerDocument.PublicKey)).Returns(innerDocument);

            var target = new QuestionnaireDenormalizer(documentStorage.Object, new CompleteQuestionFactory());
            
            var evnt = new QuestionChanged
                {
                    QuestionType = QuestionType.AutoPropagate,
                    PublicKey = question.PublicKey, 
                    Featured = true, 
                    MaxValue = 10, 
                    Triggers = triggers 
                };
            IPublishedEvent<QuestionChanged> e =
                new PublishedEvent<QuestionChanged>(
                    new UncommittedEvent(
                        Guid.NewGuid(), 
                        innerDocument.PublicKey, 
                        1, 
                        1, 
                        DateTime.Now, 
                        evnt, 
                        new Version(1, 0)));

            target.Handle(e);

            var resultQuestion = group.Find<AutoPropagateQuestion>(question.PublicKey);

            Assert.AreEqual(evnt.Featured, resultQuestion.Featured);
            Assert.AreEqual(evnt.MaxValue, resultQuestion.MaxValue);
            Assert.AreEqual(evnt.Triggers.Count, resultQuestion.Triggers.Count);
            Assert.IsTrue(resultQuestion.Triggers.Except(triggers).Count() == 0, "Triiggers list is not updated");
        }
    }
}