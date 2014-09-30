using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;

namespace WB.Core.BoundedContexts.Designer.Tests.PdfTests
{
    [TestFixture]
    public class PdfQuestionViewTests
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [Test]
        public void GetReadableValidationExpression_When_question_has_correct_validation_expression_Then_questionId_is_replaced()
        {
            // Arrange
            var groupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var group = CreatePdfGroupView(groupId);
            var questionId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var validationExpression = string.Format("[{0}]>0", questionId);
            var expectedValidationExpression = string.Format("[{0}]>0", "question 1");
            var stataCaption = "q1";
            var pdfQuestion = CreatePdfQuestionView(questionId, stataCaption, string.Empty, validationExpression);
            group.AddChild(pdfQuestion);
            
            // Act
            var transformedValidation = pdfQuestion.GetReadableValidationExpression();

            // Assert
            Assert.That(transformedValidation, Is.EqualTo(expectedValidationExpression));
        }

        [Test]
        public void GetReadableValidationExpression_When_question_has_empty_validation_expression_Then_null_is_returned()
        {
            // Arrange
            var groupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var group = CreatePdfGroupView(groupId);
            var questionId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var validationExpression = string.Empty;
            var stataCaption = "q1";
            var pdfQuestion = CreatePdfQuestionView(questionId, stataCaption, string.Empty, validationExpression);
            group.AddChild(pdfQuestion);

            // Act
            var transformedValidation = pdfQuestion.GetReadableValidationExpression();

            // Assert
            Assert.That(transformedValidation, Is.Null);
        }


        [Test]
        public void GetReadableValidationExpression_When_validation_contains_unknown_questionId_Then_correct_unknown_question_title_is_used()
        {
            // Arrange
            var groupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var group = CreatePdfGroupView(groupId);

            var questionId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var validationExpression = string.Format("[{0}]>0", Guid.NewGuid());
            var expectedValidationExpression = string.Format("[{0}]>0", "unknown question");

            var stataCaption = "q1";
            var pdfQuestion = CreatePdfQuestionView(questionId, stataCaption, string.Empty, validationExpression);
            group.AddChild(pdfQuestion);

            // Act
            var transformedValidation = pdfQuestion.GetReadableValidationExpression();

            // Assert
            Assert.That(transformedValidation, Is.EqualTo(expectedValidationExpression));
        }

        [Test]
        public void GetReadableConditionExpression_When_question_has_correct_validation_expression_Then_questionId_is_replaced()
        {
            // Arrange
            var groupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var group = CreatePdfGroupView(groupId);
            var questionId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var conditionExpression = string.Format("[{0}]>0", questionId);
            var expectedConditionExpression = string.Format("[{0}]>0", "question 1");
            var stataCaption = "q1";
            var pdfQuestion = CreatePdfQuestionView(questionId, stataCaption, conditionExpression, string.Empty);
            group.AddChild(pdfQuestion);

            // Act
            var transformedValidation = pdfQuestion.GetReadableConditionExpression();

            // Assert
            Assert.That(transformedValidation, Is.EqualTo(expectedConditionExpression));
        }

        [Test]
        public void GetReadableConditionExpression_When_question_has_empty_validation_expression_Then_null_is_returned()
        {
            // Arrange
            var groupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var group = CreatePdfGroupView(groupId);
            var questionId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var conditionExpression = string.Empty;
            var stataCaption = "q1";
            var pdfQuestion = CreatePdfQuestionView(questionId, stataCaption, conditionExpression, string.Empty);
            group.AddChild(pdfQuestion);

            // Act
            var transformedValidation = pdfQuestion.GetReadableConditionExpression();

            // Assert
            Assert.That(transformedValidation, Is.Null);
        }


        [Test]
        public void GetReadableConditionExpression_When_validation_contains_unknown_questionId_Then_correct_unknown_question_title_is_used()
        {
            // Arrange
            var groupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var group = CreatePdfGroupView(groupId);

            var questionId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var conditionExpression = string.Format("[{0}]>0", Guid.NewGuid());
            var expectedValidationExpression = string.Format("[{0}]>0", "unknown question");

            var stataCaption = "q1";
            var pdfQuestion = CreatePdfQuestionView(questionId, stataCaption, conditionExpression, string.Empty);
            group.AddChild(pdfQuestion);

            // Act
            var transformedValidation = pdfQuestion.GetReadableConditionExpression();

            // Assert
            Assert.That(transformedValidation, Is.EqualTo(expectedValidationExpression));
        }

        [Test]
        public void GetStringItemNumber_When_question_in_group_Then_correct_item_number_equals_00001()
        {
            // Arrange
            var groupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var group = CreatePdfGroupView(groupId);

            var questionId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            
            var expectedItemNumber = "00001";

            var pdfQuestion = CreatePdfQuestionView(questionId, string.Empty, string.Empty, string.Empty);
            group.AddChild(pdfQuestion);

            // Act
            var transformedValidation = pdfQuestion.GetStringItemNumber();

            // Assert
            Assert.That(transformedValidation, Is.EqualTo(expectedItemNumber));
        }

        [Test]
        public void GetHasCondition_When_question_has_empty_conditions_Then_method_returs_false()
        {
            // Arrange
            var questionId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var expectedHasCondition = false;
            var pdfQuestion = CreatePdfQuestionView(questionId, string.Empty, string.Empty, string.Empty);

            // Act
            var transformedValidation = pdfQuestion.GetHasCondition();

            // Assert
            Assert.That(transformedValidation, Is.EqualTo(expectedHasCondition));
        }


        private static PdfQuestionView CreatePdfQuestionView(Guid publicKey, string stataCaption, string conditionExpression, string validationExpression)
        {
            var newQuestion = new PdfQuestionView
            {
                PublicId = publicKey,
                Title = "question text",
                QuestionType = QuestionType.Text,
                Answers = new List<PdfAnswerView>(),
                VariableName = stataCaption,
                ValidationExpression = validationExpression,
                ConditionExpression = conditionExpression
            };

            return newQuestion;
        }

        private static PdfGroupView CreatePdfGroupView(Guid publicKey)
        {
            return new PdfGroupView() { PublicId = publicKey };
        }
    }
}
