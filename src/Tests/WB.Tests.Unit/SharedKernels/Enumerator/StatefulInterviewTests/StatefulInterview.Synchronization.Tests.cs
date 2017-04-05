using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

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

            interview.Synchronize(Create.Command.Synchronize(Guid.NewGuid(), syncDto));

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
            interview.Synchronize(Create.Command.Synchronize(Guid.NewGuid(), syncDto));

            //assert
            var multiLinkedToList = interview.GetMultiOptionLinkedToListQuestion(Create.Entity.Identity(multiLinkedToListId, Create.Entity.RosterVector(1)));

            Assert.That(multiLinkedToList.Options, Is.EquivalentTo(new[] {1, 5}));
        }

        [Test]
        public void When_rejected_interview_has_comment_for_removed_answer_Should_skip_that_comment_and_successfully_sync_without_exception()
        {
            //arrange
            var textListQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var textQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var groupId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var staticTextId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextListQuestion(textListQuestionId),
                Create.Entity.ListRoster(rosterSizeQuestionId: textListQuestionId, children: new IComposite[]
                {
                    Create.Entity.TextQuestion(textQuestionId),
                    Create.Entity.Group(groupId),
                    Create.Entity.StaticText(staticTextId)
                }));

            var interview = Setup.StatefulInterview(questionnaire);

            var answerOnTextList = Create.Entity.AnsweredQuestionSynchronizationDto(
                questionId: textListQuestionId,
                answer: new[] { new Tuple<decimal, string>(2, "option 2"), new Tuple<decimal, string>(5, "option 5") });

            var answerTextQuestion = Create.Entity.AnsweredQuestionSynchronizationDto(
                questionId: textQuestionId, 
                rosterVector:Create.Entity.RosterVector(1),
                answer: null, 
                comments: Create.Entity.CommentSynchronizationDto("comment"));

            var groupIdentity = Create.Entity.InterviewItemId( groupId, Create.Entity.RosterVector(1));
            var questionIdentity = Create.Entity.Identity( groupId, Create.Entity.RosterVector(1));
            var staticTextIdentity = Create.Entity.Identity( groupId, Create.Entity.RosterVector(1));

            var failedValidationConditions = new List<FailedValidationCondition> { Create.Entity.FailedValidationCondition(0) };

            var syncDto = Create.Entity.InterviewSynchronizationDto(
                answers: new[] { answerOnTextList, answerTextQuestion },
                disabledGroups: new HashSet<InterviewItemId> { groupIdentity },
                disabledQuestions: new HashSet<InterviewItemId> { groupIdentity },
                failedValidationConditions: new  Dictionary<Identity, IList<FailedValidationCondition>>
                {
                    { questionIdentity, failedValidationConditions } 
                },
                disabledStaticTexts: new List<Identity> { staticTextIdentity },
                invalidStaticTexts: new List<KeyValuePair<Identity, List<FailedValidationCondition>>>
                {
                    new KeyValuePair<Identity, List<FailedValidationCondition>>(staticTextIdentity, failedValidationConditions)
                }
               );

            //act
            TestDelegate sync = () => interview.ApplyEvent(new InterviewSynchronized(syncDto));

            //assert
            Assert.DoesNotThrow(sync);
        }
    }
}