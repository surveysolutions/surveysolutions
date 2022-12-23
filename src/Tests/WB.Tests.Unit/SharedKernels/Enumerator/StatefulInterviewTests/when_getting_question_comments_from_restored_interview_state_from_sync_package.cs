using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_getting_commented_by_supervisor_questions_from_restored_from_sync_package_interview : StatefulInterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(new[]
            {
                Create.Entity.FixedRoster(fixedTitles: new[] {Create.Entity.FixedTitle(1)}, children: new[]
                {
                    Create.Entity.FixedRoster(fixedTitles: new[] {Create.Entity.FixedTitle(0)}, children: new[]
                    {
                        Create.Entity.NumericIntegerQuestion(superQuestionId, scope: QuestionScope.Supervisor),
                        Create.Entity.NumericIntegerQuestion(prefilledQuestionId, isPrefilled: true),
                        Create.Entity.NumericIntegerQuestion(hiddenQuestionId, scope: QuestionScope.Hidden),
                        Create.Entity.NumericIntegerQuestion(disabledQuestionId),
                        Create.Entity.NumericIntegerQuestion(interQuestionId),
                        Create.Entity.NumericIntegerQuestion(repliedByInterQuestionId),
                    })
                })
            });

            interview = Create.AggregateRoot.StatefulInterview(userId: userId,
                questionnaireRepository: Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire));

            var answersDtos = new[]
            {
                Create.Entity.AnsweredQuestionSynchronizationDto(superQuestionId, rosterVector, 1, Create.Entity.CommentSynchronizationDto(userRole: UserRoles.Supervisor)),
                Create.Entity.AnsweredQuestionSynchronizationDto(prefilledQuestionId, rosterVector, 2, Create.Entity.CommentSynchronizationDto(userRole: UserRoles.Supervisor)),
                Create.Entity.AnsweredQuestionSynchronizationDto(hiddenQuestionId, rosterVector, 3, Create.Entity.CommentSynchronizationDto(userRole: UserRoles.Supervisor)),
                Create.Entity.AnsweredQuestionSynchronizationDto(disabledQuestionId, rosterVector, 4, Create.Entity.CommentSynchronizationDto(userRole: UserRoles.Supervisor)),
                Create.Entity.AnsweredQuestionSynchronizationDto(repliedByInterQuestionId, rosterVector, 5, Create.Entity.CommentSynchronizationDto(userRole: UserRoles.Supervisor), Create.Entity.CommentSynchronizationDto(userRole: UserRoles.Interviewer)),
                Create.Entity.AnsweredQuestionSynchronizationDto(interQuestionId, rosterVector, 6, Create.Entity.CommentSynchronizationDto(userRole: UserRoles.Supervisor))
            };

            var interviewSynchronizationDto = Create.Entity.InterviewSynchronizationDto(
                questionnaireId: questionnaireId,
                userId: userId,
                answers: answersDtos,
                disabledQuestions: new HashSet<InterviewItemId> { Create.Entity.InterviewItemId(disabledQuestionId, rosterVector) });
            interview.Synchronize(Create.Command.Synchronize(userId, interviewSynchronizationDto));
            interview.Apply(Create.Event.QuestionsDisabled(new []{ Create.Identity(disabledQuestionId, rosterVector) }));

            BecauseOf();
        }

        private void BecauseOf() =>
            commentedQuestionsIdentities = interview.GetCommentedBySupervisorQuestionsVisibleToInterviewer().ToArray();

        [NUnit.Framework.Test] public void should_return_2_commented_by_supervisor_questions () =>
            commentedQuestionsIdentities.Length.Should().Be(2);

        [NUnit.Framework.Test] public void should_return_only_interviewers_questions () =>
            commentedQuestionsIdentities.Select(x => x.Id)
                .Should().BeEquivalentTo(repliedByInterQuestionId, interQuestionId);

        [NUnit.Framework.Test] public void should_return_question_without_interviewer_interviewer_reply_first () =>
            commentedQuestionsIdentities.ElementAt(0).Id.Should().Be(interQuestionId);

        private static StatefulInterview interview;
        private static Identity[] commentedQuestionsIdentities;
        private static readonly Guid superQuestionId = Guid.Parse("00000000000000000000000000000001");
        private static readonly Guid prefilledQuestionId = Guid.Parse("00000000000000000000000000000002");
        private static readonly Guid hiddenQuestionId = Guid.Parse("00000000000000000000000000000003");
        private static readonly Guid disabledQuestionId = Guid.Parse("00000000000000000000000000000006");
        private static readonly Guid interQuestionId = Guid.Parse("00000000000000000000000000000007");
        private static readonly Guid repliedByInterQuestionId = Guid.Parse("00000000000000000000000000000008");
        private static readonly RosterVector rosterVector = Create.RosterVector(1, 0);
        private static readonly Guid questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid userId = Guid.Parse("99999999999999999999999999999999");
    }
}
