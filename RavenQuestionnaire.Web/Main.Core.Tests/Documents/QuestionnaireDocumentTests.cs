using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using Moq;
using NUnit.Framework;

namespace Main.Core.Tests.Documents
{
    [TestFixture]
    public class QuestionnaireDocumentTests
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [Test]
        public void RemoveGroup_when_not_AutoPropagate_group_removed_then_count_of_triggers_in_AutoPropagate_question_should_be_the_same()
        {
            // Arrange
            var notAutoPropagateGroupId = Guid.NewGuid();
            AutoPropagateQuestion autoQuestion;
            var doc = this.CreateQuestionnaireDocumentWithRegularGroupAndAutoPropagateQuestion(notAutoPropagateGroupId, out autoQuestion);
            var expectedCountOfTriggers = autoQuestion.Triggers.Count;

            // Act
            doc.RemoveGroup(notAutoPropagateGroupId);

            // Assert
            Assert.That(autoQuestion.Triggers.Count, Is.EqualTo(expectedCountOfTriggers));
        }

        private QuestionnaireDocument CreateQuestionnaireDocumentWithRegularGroupAndAutoPropagateQuestion(Guid groupId, out AutoPropagateQuestion autoQuestion)
        {
            var doc = new QuestionnaireDocument();
            var chapter1 = new Group("Chapter 1") { PublicKey = Guid.NewGuid() };
            autoQuestion = new AutoPropagateQuestion("Auto question") { PublicKey = Guid.NewGuid(), MaxValue = 10, Triggers = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() } };
            var chapter2 = new Group("Chapter 2") { PublicKey = Guid.NewGuid() };
            var autoGroup = new Group("Auto Group") { PublicKey = groupId, Propagated = Propagate.None };

            chapter1.Children.Add(autoQuestion);
            chapter2.Children.Add(autoGroup);

            doc.Children.Add(chapter1);
            doc.Children.Add(chapter2);

            return doc;
        }

        [Test]
        public void RemoveHeadPropertiesFromRosters_when_removeRosterTitle_is_called_then_allrosters_titleQuestionId_is_CleanedUp()
        {
            // Arrange
            var questionId = Guid.NewGuid();
            var question1Id = Guid.NewGuid();
            var group1Id = Guid.NewGuid();
            var group2Id = Guid.NewGuid();
            
            var doc = CreateQuestionnaireDocumentWithTwoRostersAndTwoRosterTitleQuestion(questionId, question1Id, group1Id, group2Id);

            // Act
            doc.RemoveHeadPropertiesFromRosters(questionId);

            // Assert
            Assert.That(doc.Find<IGroup>(x => x.PublicKey == group2Id).FirstOrDefault().RosterTitleQuestionId, Is.Null);
            Assert.That(doc.Find<IGroup>(x => x.PublicKey == group1Id).FirstOrDefault().RosterTitleQuestionId, Is.Null);
        }

        [Test]
        public void MoveHeadQuestionPropertiesToRoster_when_headquestion_is_removed_then_allrosters_titleQuestionIds_are_CleanedUp()
        {
            // Arrange
            var questionId = Guid.NewGuid();
            var question1Id = Guid.NewGuid();
            var group1Id = Guid.NewGuid();
            var group2Id = Guid.NewGuid();


            var doc = CreateQuestionnaireDocumentWithTwoRostersAndTwoRosterTitleQuestion(questionId, question1Id, group1Id, group2Id);

            // Act
            doc.MoveHeadQuestionPropertiesToRoster(question1Id, group1Id);

            // Assert
            Assert.That(doc.Find<IGroup>(x => x.PublicKey == group1Id).FirstOrDefault().RosterTitleQuestionId, Is.EqualTo(question1Id));
            Assert.That(doc.Find<IGroup>(x => x.PublicKey == group2Id).FirstOrDefault().RosterTitleQuestionId, Is.EqualTo(question1Id));
        }


        private QuestionnaireDocument CreateQuestionnaireDocumentWithTwoRostersAndTwoRosterTitleQuestion(Guid textQuestionId, Guid textQuestion1Id, Guid group1Id, Guid group2Id)
        {
            var doc = new QuestionnaireDocument();
            var chapter1 = new Group("Chapter 1") { PublicKey = Guid.NewGuid() };

            var numQuestionId = Guid.NewGuid();
            //var textQuestionId = Guid.NewGuid();

            var numQuestion = new NumericQuestion("Numeric") { PublicKey = numQuestionId, MaxValue = 10 };
            
            var textQuestion = new TextQuestion("Text") { PublicKey = textQuestionId };
            var textQuestion1 = new TextQuestion("Text") { PublicKey = textQuestion1Id };
            

            var group1 = new Group("G 1")
            {
                PublicKey = group1Id,
                RosterTitleQuestionId = textQuestionId, 
                IsRoster = true, 
                RosterSizeQuestionId = numQuestionId };

            var group2 = new Group("G 2")
            {
                PublicKey = group2Id,
                RosterTitleQuestionId = textQuestionId, 
                IsRoster = true, 
                RosterSizeQuestionId = numQuestionId };

            group1.Children.Add(textQuestion);
            group2.Children.Add(textQuestion1);

            chapter1.Children.Add(numQuestion);
            chapter1.Children.Add(group1);
            chapter1.Children.Add(group2);
            
            doc.Children.Add(chapter1);

            return doc;
        }

        private QuestionnaireDocument CreateQuestionnaireDocumentWithAutoPropagateGroupAndQuestion(Guid autoGroupId, out AutoPropagateQuestion autoQuestion)
        {
            var doc = new QuestionnaireDocument();
            var chapter1 = new Group("Chapter 1") { PublicKey = Guid.NewGuid() };
            autoQuestion = new AutoPropagateQuestion("Auto question") { PublicKey = Guid.NewGuid(), MaxValue = 10, Triggers = new List<Guid> { autoGroupId } };
            var chapter2 = new Group("Chapter 2") { PublicKey = Guid.NewGuid() };
            var autoGroup = new Group("Auto Group") { PublicKey = autoGroupId, Propagated = Propagate.AutoPropagated };

            chapter1.Children.Add(autoQuestion);
            chapter2.Children.Add(autoGroup);

            doc.Children.Add(chapter1);
            doc.Children.Add(chapter2);

            return doc;
        }
    }
}
