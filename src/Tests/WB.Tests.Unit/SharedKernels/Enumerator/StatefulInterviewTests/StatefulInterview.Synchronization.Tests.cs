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

        [Test]
        public void When_rejected_interview_has_linked_to_list_and_text_list_question_is_answered_Should_linked_to_list_question_has_options()
        {
            //arrange
            var textListQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var multiLinkedToListId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextListQuestion(textListQuestionId),
                Create.Entity.FixedRoster(fixedTitles: new[] {Create.Entity.FixedTitle(1)},
                    children: new[]
                    {
                        Create.Entity.MultyOptionsQuestion(multiLinkedToListId, linkedToQuestionId: textListQuestionId)
                    }));

            var interview = Setup.StatefulInterview(questionnaire);

            var answerOnTextList = Create.Entity.AnsweredQuestionSynchronizationDto(questionId: textListQuestionId,
                answer: new[] {new Tuple<decimal, string>(1, "option 1"), new Tuple<decimal, string>(5, "option 2")});

            var syncDto = Create.Entity.InterviewSynchronizationDto(answers: new[] { answerOnTextList });

            //act
            interview.SynchronizeInterview(Guid.NewGuid(), syncDto);

            //assert
            var multiLinkedToList = interview.GetMultiOptionLinkedToListQuestion(Create.Entity.Identity(multiLinkedToListId, Create.Entity.RosterVector(1)));

            Assert.That(multiLinkedToList.Options, Is.EquivalentTo(new[] {1, 5}));
        }
    }
}