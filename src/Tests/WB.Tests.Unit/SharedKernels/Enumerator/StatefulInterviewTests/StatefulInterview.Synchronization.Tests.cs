using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal partial class StatefullInterviewTests
    {
        [Test]
        public void When_unanswered_question_has_comment_Should_add_to_comments_of_question()
        {
            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneQuestion(questionId);

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: Create.Entity.PlainQuestionnaire(questionnaire));

            var commentedAnswer = Create.Entity.AnsweredQuestionSynchronizationDto(questionId: questionId,
                comments: Create.Entity.CommentSynchronizationDto(userRole: UserRoles.Supervisor));

            var syncDto = Create.Entity.InterviewSynchronizationDto(
                answers: new[] {commentedAnswer});

            interview.SynchronizeInterview(Guid.NewGuid(), syncDto);

            var questionComments = interview.GetQuestionComments(Create.Entity.Identity(questionId)).ToList();

            Assert.That(questionComments, Is.Not.Empty);
            Assert.That(questionComments, Has.Count.EqualTo(1));
        }
    }
}