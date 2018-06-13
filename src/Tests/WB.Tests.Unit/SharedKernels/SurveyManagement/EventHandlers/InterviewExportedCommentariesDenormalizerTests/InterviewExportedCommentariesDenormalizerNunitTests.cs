using System;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.InterviewExportedCommentariesDenormalizerTests
{
    [TestFixture]
    internal class InterviewExportedCommentariesDenormalizerNunitTests
    {
        private InterviewExportedCommentariesDenormalizer CreateInterviewExportedCommentariesDenormalizer(
            IReadSideRepositoryWriter<InterviewCommentaries> interviewCommentariesStorage = null,
            IUserViewFactory userStorage = null,
            IQuestionnaireExportStructureStorage questionnaireReader = null)
        {
            return new InterviewExportedCommentariesDenormalizer(
                interviewCommentariesStorage ?? new TestInMemoryWriter<InterviewCommentaries>(),
                userStorage ?? Mock.Of<IUserViewFactory>(),
                questionnaireReader??Mock.Of<IQuestionnaireExportStructureStorage>());
        }

        [Test]
        public void Handle_When_InterviewApprovedByHQ_event_arrived_with_empty_comment_Then_interview_should_be_marked_as_approved()
        {
            var interviewId = Guid.NewGuid();
            var interviewCommentaries = Create.Entity.InterviewCommentaries();
            var interviewCommentariesStorage = new TestInMemoryWriter<InterviewCommentaries>();
            interviewCommentariesStorage.Store(interviewCommentaries, interviewId);

            var interviewExportedCommentariesDenormalizer = CreateInterviewExportedCommentariesDenormalizer(interviewCommentariesStorage: interviewCommentariesStorage);

            interviewExportedCommentariesDenormalizer.Handle(Create.PublishedEvent.InterviewApprovedByHQ(interviewId: interviewId));

            Assert.That(interviewCommentaries.IsApprovedByHQ, Is.True);
            Assert.That(interviewCommentaries.Commentaries.Count, Is.EqualTo(0));
        }

        [Test]
        public void Handle_When_InterviewApprovedByHQ_event_arrived_with_not_empty_comment_Then_interview_should_be_marked_as_approved()
        {
            var interviewId = Guid.NewGuid();
            var comment = "comment";
            var interviewCommentaries = Create.Entity.InterviewCommentaries();
            var interviewCommentariesStorage = new TestInMemoryWriter<InterviewCommentaries>();
            interviewCommentariesStorage.Store(interviewCommentaries, interviewId);

            var interviewExportedCommentariesDenormalizer = CreateInterviewExportedCommentariesDenormalizer(interviewCommentariesStorage: interviewCommentariesStorage);

            interviewExportedCommentariesDenormalizer.Handle(Create.PublishedEvent.InterviewApprovedByHQ(interviewId: interviewId, comment:comment));

            Assert.That(interviewCommentaries.IsApprovedByHQ, Is.True);
            Assert.That(interviewCommentaries.Commentaries[0].Comment, Is.EqualTo(comment));
        }
    }
}