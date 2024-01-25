using System;
using System.Collections.Generic;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    internal partial class ImportDataVerifierTests
    {
        [Test]
        public void when_verify_answers_on_interview_tree_and_questionnaire_has_no_question_Should_report_error()
        {
            var questionnaireDocument = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneQuestion());
            var verifier = Create.Service.ImportDataVerifier();

            var answers = new List<InterviewAnswer>
            {
                Create.Entity.InterviewAnswer(Create.Identity(), Create.Entity.TextQuestionAnswer("blabla"))
            };

            var verificationError = verifier.VerifyWithInterviewTree(answers, null, questionnaireDocument);

            Assert.That(verificationError, Is.Not.Null);
        }

        [Test]
        public void when_verify_answers_on_interview_tree_and_answer_on_cascading_question_but_no_answer_on_parent_question_should_report_specified_error()
        {
            // arrange
            var parentCascadingQuestionId = Guid.Parse("11111111111111111111111111111111");
            var cascadingQuestionId = Guid.Parse("22222222222222222222222222222222");

            var answerOnParentQuestion = 99999;
            var answer = 9999;
            var expectedParentValue = 0;

            var optionsRepository = Create.Storage.QuestionnaireQuestionOptionsRepository();

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.SingleOptionQuestion(parentCascadingQuestionId, answerCodes: new[] { answerOnParentQuestion, answer }),
                    Create.Entity.SingleOptionQuestion(cascadingQuestionId,
                        cascadeFromQuestionId: parentCascadingQuestionId, answerCodes: new[] { answerOnParentQuestion, answer },
                        parentCodes: new[] { 1, expectedParentValue })),
                version: 1,
                questionOptionsRepository: optionsRepository);

            var verifier = Create.Service.ImportDataVerifier(
                interviewTreeBuilder: Create.Service.InterviewTreeBuilder(),
                optionsRepository: optionsRepository);

            var answers = new List<InterviewAnswer>
            {
                Create.Entity.InterviewAnswer(Create.Identity(parentCascadingQuestionId), Create.Entity.SingleOptionAnswer((int)answerOnParentQuestion)),
                Create.Entity.InterviewAnswer(Create.Identity(cascadingQuestionId), Create.Entity.SingleOptionAnswer((int)answer))
            };

            // act
            var verificationError = verifier.VerifyWithInterviewTree(answers, null, questionnaire);

            // assert
            Assert.That(verificationError.ErrorMessage, Does.Contain("PL0011"));
            Assert.That(verificationError.ErrorMessage, Does.Contain($"Error during import of assignment with answers: {answerOnParentQuestion}, {answer}."));
            Assert.That(verificationError.ErrorMessage, Does.Contain("Exception: Provided answer is not in the list part of predefined answers"));
            Assert.That(verificationError.ErrorMessage, Does.Contain($"Question ID: {cascadingQuestionId}"));
            Assert.That(verificationError.ErrorMessage, Does.Contain($"ProvidedAnswer: {answer}"));
            Assert.That(verificationError.ErrorMessage, Does.Contain($"ParentValue: {answerOnParentQuestion}"));
        }
    }
}
