using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using Moq;
using NUnit.Framework;
using WB.Tests.Unit;

namespace Main.Core.Tests.Documents
{
    [TestFixture]
    public class QuestionnaireDocumentTests
    {
        [SetUp]
        public void SetUp()
        {
            AssemblyContext.SetupServiceLocator();
        }

        [Test]
        public void RemoveHeadPropertiesFromRosters_when_removeRosterTitle_is_called_then_allrosters_titleQuestionId_is_CleanedUp()
        {
            // Arrange
            var questionId = Guid.Parse("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var question1Id = Guid.Parse("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB1");
            var roster1Id = Guid.Parse("CDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD1");
            var roster2Id = Guid.Parse("CDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD2");
            var roster3Id = Guid.Parse("CDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD3");

            var group1Id = Guid.Parse("BBDDDDDDDDDDDDDDDDDDDDDDDDDDDDD3");

            var doc = CreateQuestionnaireDocumentWithTreeRostersOneGroupAndTwoRosterTitleQuestion(questionId, question1Id, roster1Id, roster2Id, roster3Id, group1Id);

            // Act
            doc.RemoveHeadPropertiesFromRosters(questionId);

            // Assert
            Assert.That(doc.Find<IGroup>(x => x.PublicKey == roster2Id).FirstOrDefault().RosterTitleQuestionId, Is.Null);
            Assert.That(doc.Find<IGroup>(x => x.PublicKey == roster1Id).FirstOrDefault().RosterTitleQuestionId, Is.Null);
        }

        [Test]
        public void MoveHeadQuestionPropertiesToRoster_when_headquestion_is_removed_then_allrosters_titleQuestionIds_are_Updated()
        {
            // Arrange
            var questionId = Guid.Parse("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var question1Id = Guid.Parse("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB1");
            var roster1Id = Guid.Parse("CDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD1");
            var roster2Id = Guid.Parse("CDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD2");
            var roster3Id = Guid.Parse("CDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD3");

            var group1Id = Guid.Parse("BBDDDDDDDDDDDDDDDDDDDDDDDDDDDDD3");

            var doc = CreateQuestionnaireDocumentWithTreeRostersOneGroupAndTwoRosterTitleQuestion(questionId, question1Id, roster1Id, roster2Id, roster3Id, group1Id);

            // Act
            doc.MoveHeadQuestionPropertiesToRoster(question1Id, roster1Id);

            // Assert
            Assert.That(doc.Find<IGroup>(x => x.PublicKey == roster1Id).FirstOrDefault().RosterTitleQuestionId, Is.EqualTo(question1Id));
            Assert.That(doc.Find<IGroup>(x => x.PublicKey == roster2Id).FirstOrDefault().RosterTitleQuestionId, Is.EqualTo(question1Id));
        }

        [Test]
        public void CheckIsQuestionHeadAndUpdateRosterProperties_When_head_question_moved_from_group_Then_RosterTitleQuestionId_are_cleared()
        {
            // Arrange
            var questionId = Guid.Parse("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var question1Id = Guid.Parse("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB1");
            var roster1Id = Guid.Parse("CDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD1");
            var roster2Id = Guid.Parse("CDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD2");
            var roster3Id = Guid.Parse("CDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD3");

            var group1Id = Guid.Parse("BBDDDDDDDDDDDDDDDDDDDDDDDDDDDDD3");

            var doc = CreateQuestionnaireDocumentWithTreeRostersOneGroupAndTwoRosterTitleQuestion(questionId, question1Id, roster1Id, roster2Id, roster3Id, group1Id);
            doc.MoveItem(questionId, roster3Id, 0);

            // Act
            doc.CheckIsQuestionHeadAndUpdateRosterProperties(questionId, roster3Id);

            // Assert
            Assert.That(doc.Find<IGroup>(x => x.PublicKey == roster2Id).FirstOrDefault().RosterTitleQuestionId, Is.Null);
            Assert.That(doc.Find<IGroup>(x => x.PublicKey == roster1Id).FirstOrDefault().RosterTitleQuestionId, Is.Null);
        }

        [Test]
        public void CheckIsQuestionHeadAndUpdateRosterProperties_When_head_question_moved_to_another_group_Then_RosterTitleQuestionId_is_Updated()
        {
            // Arrange
            var questionId = Guid.Parse("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var question1Id = Guid.Parse("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB1");
            var roster1Id = Guid.Parse("CDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD1");
            var roster2Id = Guid.Parse("CDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD2");
            var roster3Id = Guid.Parse("CDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD3");

            var group1Id = Guid.Parse("BBDDDDDDDDDDDDDDDDDDDDDDDDDDDDD3");

            var doc = CreateQuestionnaireDocumentWithTreeRostersOneGroupAndTwoRosterTitleQuestion(questionId, question1Id, roster1Id, roster2Id, roster3Id, group1Id);
            doc.MoveItem(questionId, roster3Id, 0);

            // Act
            doc.CheckIsQuestionHeadAndUpdateRosterProperties(questionId, roster3Id);

            // Assert
            Assert.That(doc.Find<IGroup>(x => x.PublicKey == roster3Id).FirstOrDefault().RosterTitleQuestionId, Is.EqualTo(questionId));
        }

        [Test]
        public void CheckIsQuestionHeadAndUpdateRosterProperties_When_head_question_moved_to_another_group_and_groupId_is_incorrect_Then_RosterTitleQuestionId_is_not_Updated()
        {
            // Arrange
            var questionId = Guid.Parse("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var question1Id = Guid.Parse("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB1");
            var roster1Id = Guid.Parse("CDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD1");
            var roster2Id = Guid.Parse("CDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD2");
            var roster3Id = Guid.Parse("CDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD3");

            var group1Id = Guid.Parse("BBDDDDDDDDDDDDDDDDDDDDDDDDDDDDD3");

            var groupId_dummy = Guid.Parse("CDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD0");

            var doc = CreateQuestionnaireDocumentWithTreeRostersOneGroupAndTwoRosterTitleQuestion(questionId, question1Id, roster1Id, roster2Id, roster3Id, group1Id);
            doc.MoveItem(questionId, roster3Id, 0);

            // Act
            doc.CheckIsQuestionHeadAndUpdateRosterProperties(questionId, groupId_dummy);

            // Assert
            Assert.That(doc.Find<IGroup>(x => x.PublicKey == roster3Id).FirstOrDefault().RosterTitleQuestionId, Is.Null);
        }

        [Test]
        public void CheckIsQuestionHeadAndUpdateRosterProperties_When_head_question_moved_to_another_non_roster_group_Then_RosterTitleQuestionId_is_not_Updated()
        {
            // Arrange
            var questionId = Guid.Parse("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var question1Id = Guid.Parse("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB1");
            var roster1Id = Guid.Parse("CDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD1");
            var roster2Id = Guid.Parse("CDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD2");
            var roster3Id = Guid.Parse("CDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD3");

            var group1Id = Guid.Parse("BBDDDDDDDDDDDDDDDDDDDDDDDDDDDDD3");

            var doc = CreateQuestionnaireDocumentWithTreeRostersOneGroupAndTwoRosterTitleQuestion(questionId, question1Id, roster1Id, roster2Id, roster3Id, group1Id);
            doc.MoveItem(questionId, roster3Id, 0);

            // Act
            doc.CheckIsQuestionHeadAndUpdateRosterProperties(questionId, group1Id);

            // Assert
            Assert.That(doc.Find<IGroup>(x => x.PublicKey == group1Id).FirstOrDefault().RosterTitleQuestionId, Is.Null);
        }


        [Test]
        public void CheckIsQuestionHeadAndUpdateRosterProperties_When_head_question_moved_to_another_group_and_groupId_provided_as_null_Then_RosterTitleQuestionId_is_Updated()
        {
            // Arrange
            var questionId = Guid.Parse("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var question1Id = Guid.Parse("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB1");
            var roster1Id = Guid.Parse("CDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD1");
            var roster2Id = Guid.Parse("CDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD2");
            var roster3Id = Guid.Parse("CDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD3");

            var group1Id = Guid.Parse("BBDDDDDDDDDDDDDDDDDDDDDDDDDDDDD3");

            var doc = CreateQuestionnaireDocumentWithTreeRostersOneGroupAndTwoRosterTitleQuestion(questionId, question1Id, roster1Id, roster2Id, roster3Id, group1Id);
            doc.MoveItem(questionId, roster3Id, 0);

            // Act
            doc.CheckIsQuestionHeadAndUpdateRosterProperties(questionId, null);

            // Assert
            Assert.That(doc.Find<IGroup>(x => x.PublicKey == roster3Id).FirstOrDefault().RosterTitleQuestionId, Is.EqualTo(questionId));
        }

        [Test]
        public void CheckIsQuestionHeadAndUpdateRosterProperties_When_head_question_moved_to_another_non_roster_group_and_groupId_provided_as_null_Then_RosterTitleQuestionId_is_Updated()
        {
            // Arrange
            var questionId = Guid.Parse("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var question1Id = Guid.Parse("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB1");
            var roster1Id = Guid.Parse("CDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD1");
            var roster2Id = Guid.Parse("CDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD2");
            var roster3Id = Guid.Parse("CDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD3");

            var group1Id = Guid.Parse("BBDDDDDDDDDDDDDDDDDDDDDDDDDDDDD3");

            var doc = CreateQuestionnaireDocumentWithTreeRostersOneGroupAndTwoRosterTitleQuestion(questionId, question1Id, roster1Id, roster2Id, roster3Id, group1Id);
            doc.MoveItem(questionId, roster3Id, 0);

            // Act
            doc.CheckIsQuestionHeadAndUpdateRosterProperties(questionId, null);

            // Assert
            Assert.That(doc.Find<IGroup>(x => x.PublicKey == roster3Id).FirstOrDefault().RosterTitleQuestionId, Is.EqualTo(questionId));
        }


        [Test]
        public void CheckIsQuestionHeadAndUpdateRosterProperties_When_head_question_moved_to_another_group_and_questionId_is_incorrect_Then_RosterTitleQuestionId_is_not_Updated()
        {
            // Arrange
            var questionId = Guid.Parse("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var question1Id = Guid.Parse("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB1");
            var roster1Id = Guid.Parse("CDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD1");
            var roster2Id = Guid.Parse("CDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD2");
            var roster3Id = Guid.Parse("CDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD3");

            var group1Id = Guid.Parse("BBDDDDDDDDDDDDDDDDDDDDDDDDDDDDD3");

            var questionId_dummy = Guid.Parse("AADDDDDDDDDDDDDDDDDDDDDDDDDDDDD0");

            var doc = CreateQuestionnaireDocumentWithTreeRostersOneGroupAndTwoRosterTitleQuestion(questionId, question1Id, roster1Id, roster2Id, roster3Id, group1Id);
            doc.MoveItem(questionId, roster3Id, 0);

            // Act
            doc.CheckIsQuestionHeadAndUpdateRosterProperties(questionId_dummy, null);

            // Assert
            Assert.That(doc.Find<IGroup>(x => x.PublicKey == roster3Id).FirstOrDefault().RosterTitleQuestionId, Is.Null);
        }


        private QuestionnaireDocument CreateQuestionnaireDocumentWithTreeRostersOneGroupAndTwoRosterTitleQuestion(Guid textQuestionId,
            Guid textQuestion1Id, Guid roster1Id, Guid roster2Id, Guid roster3Id, Guid group1Id)
        {
            var doc = new QuestionnaireDocument();
            var chapter1 = new Group("Chapter 1") { PublicKey = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB") };
            var numQuestionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD1");
            var numQuestion = new NumericQuestion("Numeric") { PublicKey = numQuestionId};
            var numQuestion1Id = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD2");
            var numQuestion1 = new NumericQuestion("Numeric") { PublicKey = numQuestion1Id};

            var textQuestion = new TextQuestion("Text") { PublicKey = textQuestionId, Capital = true };
            var textQuestion1 = new TextQuestion("Text") { PublicKey = textQuestion1Id, Capital = true };

            var roster1 = new Group("R 1")
            {
                PublicKey = roster1Id,
                RosterTitleQuestionId = textQuestionId, 
                IsRoster = true, 
                RosterSizeQuestionId = numQuestionId };

            var roster2 = new Group("R 2")
            {
                PublicKey = roster2Id,
                RosterTitleQuestionId = textQuestionId, 
                IsRoster = true, 
                RosterSizeQuestionId = numQuestionId };

            var roster3 = new Group("R 3")
            {
                PublicKey = roster3Id,
                RosterTitleQuestionId = null,
                IsRoster = true,
                RosterSizeQuestionId = numQuestion1Id
            };

            var group1 = new Group("G 1")
            {
                PublicKey = group1Id,
                RosterTitleQuestionId = null,
                IsRoster = false,
                RosterSizeQuestionId = numQuestion1Id
            };

            roster1.Children.Add(textQuestion);
            roster2.Children.Add(textQuestion1);

            chapter1.Children.Add(numQuestion);
            chapter1.Children.Add(numQuestion1);

            chapter1.Children.Add(roster1);
            chapter1.Children.Add(roster2);
            chapter1.Children.Add(roster3);

            chapter1.Children.Add(group1);

            doc.Children.Add(chapter1);

            return doc;
        }
    }
}
