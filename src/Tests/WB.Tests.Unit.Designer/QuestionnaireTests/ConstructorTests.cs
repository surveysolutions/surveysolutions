﻿using System;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    [TestFixture]
    internal class ConstructorTests : QuestionnaireTestsContext
    {
        [SetUp]
        public void SetUp()
        {
            
        }

        [Test]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("\t")]
        public void ctor_When_questionnaire_title_is_empty_or_contains_whitespaces_only_Then_throws_DomainException_with_type_QuestionnaireTitleRequired(string emptyTitle)
        {
            // arrange

            // act
            TestDelegate act = () =>
            {
                Questionnaire questionnaire = Create.Questionnaire();
                var command = Create.Command.CreateQuestionnaire(Guid.NewGuid(), emptyTitle, null, false);
                questionnaire.CreateQuestionnaire(command);
            };

            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.QuestionnaireTitleRequired));
        }

        [Test]
        public void ctor_When_public_key_specified_Then_raised_NewQuestionnaireCreated_event_with_same_public_key()
        {
            // arrange
            var publicKey = Guid.NewGuid();

            // act
            Questionnaire questionnaire = Create.Questionnaire();
            var command = Create.Command.CreateQuestionnaire(publicKey, "title", null, false);
            questionnaire.CreateQuestionnaire(command);

            // assert
            Assert.That(questionnaire.QuestionnaireDocument.PublicKey, Is.EqualTo(publicKey));
        }

        [Test]
        public void ctor_When_title_specified_Then_raised_NewQuestionnaireCreated_event_with_same_title()
        {
            // arrange
            var title = "title, the";

            // act
            Questionnaire questionnaire = Create.Questionnaire();
            var command = Create.Command.CreateQuestionnaire(Guid.NewGuid(), title, null, false);
            questionnaire.CreateQuestionnaire(command);

            // assert
            Assert.That(questionnaire.QuestionnaireDocument.Title, Is.EqualTo(title));
        }
    }
}