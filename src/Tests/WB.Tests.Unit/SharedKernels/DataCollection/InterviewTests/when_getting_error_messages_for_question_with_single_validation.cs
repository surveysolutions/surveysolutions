﻿using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    public class when_getting_error_messages_for_question_with_single_validation
    {
        [Test]
        public void should_not_return_message_for_valid_question()
        {
              //arrange
            var questionId = Create.Identity();

            var interview = SetUp.StatefulInterview(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(
                        id: questionId.Id,
                        validationConditions: new[] {Create.Entity.ValidationCondition(message: "error 1")})
                }));
                
            interview.Apply(Create.Event.AnswersDeclaredInvalid(questionId));
            interview.Apply(Create.Event.AnswersDeclaredValid(questionId));

            //act
            var validationMessages = interview.GetFailedValidationMessages(questionId, "default");

            //assert
            Assert.That(validationMessages, Is.Empty);
        }
    }
}