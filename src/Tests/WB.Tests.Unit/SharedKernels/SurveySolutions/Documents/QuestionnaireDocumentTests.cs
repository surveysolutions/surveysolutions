using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Tests.Unit;

namespace Main.Core.Tests.Documents
{
    [TestFixture]
    internal class QuestionnaireDocumentTests
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

        [Test]
        public void when_contains_multi_and_single_option_questions_and_parse_options_method_called()
        {
            Guid singleOptionQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            Guid multipleOptionsQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            Guid textQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var cascadingQuestionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

            var singleOptionAnswerCodes = new decimal[] {1, 2, 3};
            var multipleOptionsAnswerCodes = new [] { 4, 5, 6};
            var cascadingQuestionCodes = new decimal[] { 7, 8, 9};
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                children: new IComposite[]
                {
                    Create.Entity.SingleOptionQuestion(questionId: singleOptionQuestionId, answerCodes: singleOptionAnswerCodes),
                    Create.Entity.SingleOptionQuestion(questionId: cascadingQuestionId, 
                        cascadeFromQuestionId: singleOptionQuestionId,
                        answerCodes: cascadingQuestionCodes,
                        parentCodes: singleOptionAnswerCodes
                        ),
                    Create.Entity.MultipleOptionsQuestion(questionId: multipleOptionsQuestionId, answers: multipleOptionsAnswerCodes),
                    Create.Entity.TextQuestion(questionId: textQuestionId)
                });

            questionnaire.ParseCategoricalQuestionOptions();

            var singleQuestion = questionnaire.GetQuestion<SingleQuestion>(singleOptionQuestionId);
            Assert.That(singleQuestion.Answers.Select(x => x.AnswerCode), Is.EquivalentTo(singleOptionAnswerCodes));

            var multiOptionQuestion = questionnaire.GetQuestion<MultyOptionsQuestion>(multipleOptionsQuestionId);
            Assert.That(multiOptionQuestion.Answers.Select(x => x.AnswerCode), Is.EquivalentTo(multipleOptionsAnswerCodes));

            var cascadingQuestion = questionnaire.GetQuestion<SingleQuestion>(cascadingQuestionId);
            Assert.That(cascadingQuestion.Answers.Select(x => x.ParentCode), Is.EquivalentTo(singleOptionAnswerCodes));

            Assert.That(questionnaire.GetQuestion<TextQuestion>(textQuestionId).Answers, Is.Empty);
        }

        private QuestionnaireDocument CreateQuestionnaireDocumentWithTreeRostersOneGroupAndTwoRosterTitleQuestion(Guid textQuestionId,
            Guid textQuestion1Id, Guid roster1Id, Guid roster2Id, Guid roster3Id, Guid group1Id)
        {
            
            
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
                RosterSizeQuestionId = numQuestionId,
                Children = new IComposite[]{textQuestion}.ToReadOnlyCollection()
            };

            var roster2 = new Group("R 2")
            {
                PublicKey = roster2Id,
                RosterTitleQuestionId = textQuestionId, 
                IsRoster = true, 
                RosterSizeQuestionId = numQuestionId,
                Children = new IComposite[] { textQuestion1 }.ToReadOnlyCollection()
            };

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
            
            return new QuestionnaireDocument
            {
                Children = new IComposite[]
                {
                    new Group("Chapter 1")
                    {
                        PublicKey = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"),
                        Children = new IComposite[] {numQuestion, numQuestion1, roster1, roster2, roster3, group1}.ToReadOnlyCollection()
                    }
                }.ToReadOnlyCollection()
            };
        }
    }
}
