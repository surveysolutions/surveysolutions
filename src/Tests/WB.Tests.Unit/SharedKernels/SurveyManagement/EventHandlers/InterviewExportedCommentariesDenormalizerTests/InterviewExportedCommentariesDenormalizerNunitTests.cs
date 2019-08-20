using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.InterviewExportedCommentariesDenormalizerTests
{
    [TestFixture]
    internal class InterviewExportedCommentariesDenormalizerNunitTests
    {
        private InterviewExportedCommentariesDenormalizer CreateInterviewExportedCommentariesDenormalizer(
            IUserViewFactory userStorage = null,
            IQuestionnaireStorage questionnaireReader = null)
        {
            var questionnaireStorage = Create.Storage.QuestionnaireStorage(Create.Entity.QuestionnaireDocument());
            return new InterviewExportedCommentariesDenormalizer(
                userStorage ?? Mock.Of<IUserViewFactory>(),
                questionnaireReader ?? questionnaireStorage);
        }

        [Test]
        public void Handle_When_InterviewApprovedByHQ_event_arrived_with_empty_comment_Then_interview_should_be_marked_as_approved()
        {
            var interviewId = Guid.NewGuid();
            var interviewCommentaries = Create.Entity.InterviewSummary();

            var interviewExportedCommentariesDenormalizer = CreateInterviewExportedCommentariesDenormalizer();

            interviewCommentaries = interviewExportedCommentariesDenormalizer.Update(interviewCommentaries, Create.PublishedEvent.InterviewApprovedByHQ(interviewId: interviewId));

            Assert.That(interviewCommentaries.Comments.Count, Is.EqualTo(0));
        }

        [Test]
        public void Handle_When_InterviewApprovedByHQ_event_arrived_with_not_empty_comment_Then_interview_should_be_marked_as_approved()
        {
            var interviewId = Guid.NewGuid();
            var comment = "comment";
            var interviewCommentaries = Create.Entity.InterviewSummary();

            var interviewExportedCommentariesDenormalizer = CreateInterviewExportedCommentariesDenormalizer();

            interviewCommentaries = interviewExportedCommentariesDenormalizer.Update(
                interviewCommentaries,
                Create.PublishedEvent.InterviewApprovedByHQ(interviewId: interviewId, comment:comment));

            Assert.That(interviewCommentaries.Comments.First().Comment, Is.EqualTo(comment));
        }
    }
}
