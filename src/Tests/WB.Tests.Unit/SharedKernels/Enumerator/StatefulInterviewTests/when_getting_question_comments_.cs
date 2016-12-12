using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;

using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_getting_question_comments_from_restored_from_sync_package_interview : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            IQuestionnaireStorage questionnaireRepository =
                Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(Guid.NewGuid(),
                    Create.Entity.PlainQuestionnaire(
                        Create.Entity.QuestionnaireDocumentWithOneChapter(
                            Create.Entity.FixedRoster(fixedTitles: new[] {new FixedRosterTitle(1, "fixed")},
                                children: new[]
                                {
                                    Create.Entity.FixedRoster(
                                        fixedTitles: new[] {new FixedRosterTitle(0, "nested fixed")},
                                        children: new[]
                                        {
                                            Create.Entity.TextQuestion(questionId)
                                        })
                                }))));

            interview = Create.AggregateRoot.StatefulInterview(
                questionnaireId: questionnaireId,
                questionnaireRepository: questionnaireRepository);

            var answersDtos = new[]
            {
                Create.Entity.AnsweredQuestionSynchronizationDto(questionId, rosterVector, 1, 
                    Create.Entity.CommentSynchronizationDto(userId: supervisorId, text: "First line"),
                    Create.Entity.CommentSynchronizationDto(userId: supervisorId, text: "Second line"),
                    Create.Entity.CommentSynchronizationDto(userId: interviewerId, text: "Hello world!"),
                    Create.Entity.CommentSynchronizationDto(userId: hqId, userRole: UserRoles.Headquarter, text: "hi there!"),
                    Create.Entity.CommentSynchronizationDto(userId: interviewerId, text: "First line"),
                    Create.Entity.CommentSynchronizationDto(userId: interviewerId, text: "Second line")
                    )
            };

            interview.RestoreInterviewStateFromSyncPackage(interviewerId, Create.Entity.InterviewSynchronizationDto(
                questionnaireId: questionnaireId,
                userId: interviewerId,
                supervisorId: supervisorId,
                answers: answersDtos));
        };

        Because of = () =>
            comments = interview.GetQuestionComments(Create.Entity.Identity(questionId, rosterVector));

        It should_return_4_commens = () =>
            comments.Count().ShouldEqual(6);

        It should_first_2_comments_from_supervisor = () =>
        {
            var comment = comments.ElementAt(0);
            comment.UserId.ShouldEqual(supervisorId);
            comment.Comment.ShouldEqual($"First line");

            comment = comments.ElementAt(1);
            comment.UserId.ShouldEqual(supervisorId);
            comment.Comment.ShouldEqual($"Second line");
        };

        It should_return_third_comment_from_interviewer = () =>
        {
            var comment = comments.ElementAt(2);
            comment.UserId.ShouldEqual(interviewerId);
            comment.Comment.ShouldEqual("Hello world!");
        };

        It should_return_HQ_comment_on_4_position = () =>
        {
            var comment = comments.ElementAt(3);
            comment.UserId.ShouldEqual(hqId);
            comment.Comment.ShouldEqual("hi there!");
        };

        It should_merge_last_2_comments_from_inerviewer = () =>
        {
            var comment = comments.ElementAt(4);
            comment.UserId.ShouldEqual(interviewerId);
            comment.Comment.ShouldEqual($"First line");

            comment = comments.ElementAt(5);
            comment.UserId.ShouldEqual(interviewerId);
            comment.Comment.ShouldEqual($"Second line");
        };

        private static StatefulInterview interview;
        private static IEnumerable<AnswerComment> comments;
        private static readonly Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static readonly decimal[] rosterVector = new decimal[] { 1m, 0m };
        private static readonly Guid questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid interviewerId = Guid.Parse("99999999999999999999999999999999");
        private static readonly Guid supervisorId = Guid.Parse("88888888888888888888888888888888");
        private static readonly Guid hqId = Guid.Parse("77777777777777777777777777777777");
    }
}