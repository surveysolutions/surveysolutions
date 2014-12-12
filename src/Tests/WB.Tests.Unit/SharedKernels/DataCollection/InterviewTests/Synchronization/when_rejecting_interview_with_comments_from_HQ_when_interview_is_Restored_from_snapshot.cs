using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Snapshots;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Synchronization
{
    internal class when_rejecting_interview_with_comments_from_HQ_when_interview_is_Restored_from_snapshot : InterviewTestsContext
    {
        Establish context = () =>
        {
            interviewSynchronizationDto = new InterviewSynchronizationDto { Status = InterviewStatus.RejectedByHeadquarters };
            commentedQuestionId = Guid.NewGuid();

            existingComment = new CommentSynchronizationDto
            {
                Date = new DateTime(2009, 1, 1),
                Text = "old comment text",
                UserId = userId
            };

            newComment = new CommentSynchronizationDto
            {
                Date = new DateTime(2010, 1, 1),
                Text = "Comment text",
                UserId = userId
            };

            interviewSynchronizationDto.Answers = new[]
            {
                new AnsweredQuestionSynchronizationDto(commentedQuestionId, new decimal[] { }, "answer", null)
                {
                    AllComments = new[]
                    {
                        existingComment,
                        newComment
                    }
                }
            };

            var questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.GetHistoricalQuestionnaire(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()))
                .Returns(Mock.Of<IQuestionnaire>());

            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(questionnaireRepositoryMock.Object);

            interview = CreateInterview();


            interview.RestoreFromSnapshot(Create.InterviewState(InterviewStatus.Deleted, new List<AnswerComment>
            {
                new AnswerComment(userId, existingComment.Date, existingComment.Text, commentedQuestionId, new decimal[] { })
            }));

            eventContext = new EventContext();
        };

        Because of = () => interview.RejectInterviewFromHeadquarters(userId, Guid.NewGuid(), interviewerId, interviewSynchronizationDto, DateTime.Now);


        It should_add_new_comments = () => eventContext.ShouldContainEvent<AnswerCommented>(@event =>
            @event.Comment == newComment.Text
        );

        It should_not_add_alrady_saved_comments = () => eventContext.ShouldNotContainEvent<AnswerCommented>(@event => @event.Comment == existingComment.Text);

        static Interview interview;
        static EventContext eventContext;
        static Guid userId = Guid.NewGuid();
        private static Guid interviewerId = Guid.NewGuid();
        private static InterviewSynchronizationDto interviewSynchronizationDto;
        private static Guid commentedQuestionId;
        private static CommentSynchronizationDto newComment;
        private static CommentSynchronizationDto existingComment;
    }
}
