using System;
using Moq;
using NUnit.Framework;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.InterviewExportedCommentariesDenormalizerTests
{
    [TestFixture]
    internal class InterviewExportedCommentariesDenormalizerNunitTests
    {
        private InterviewExportedCommentariesDenormalizer CreateInterviewExportedCommentariesDenormalizer(
            IReadSideRepositoryWriter<InterviewCommentaries> interviewCommentariesStorage = null,
            IReadSideRepositoryWriter<UserDocument> userStorage = null,
            IPlainKeyValueStorage<QuestionnaireExportStructure> questionnaireReader = null)
        {
            return new InterviewExportedCommentariesDenormalizer(
                interviewCommentariesStorage ?? new TestInMemoryWriter<InterviewCommentaries>(),
                userStorage ?? new TestInMemoryWriter<UserDocument>(),
                questionnaireReader??Mock.Of<IPlainKeyValueStorage<QuestionnaireExportStructure>>());
        }

        [Test]
        public void Handle_When_InterviewApprovedByHQ_event_arrived_with_empty_comment_Then_interview_should_be_marked_as_approved()
        {
            var interviewId = Guid.NewGuid();
            var interviewCommentaries = Create.InterviewCommentaries();
            var interviewCommentariesStorage = new TestInMemoryWriter<InterviewCommentaries>();
            interviewCommentariesStorage.Store(interviewCommentaries, interviewId);

            var interviewExportedCommentariesDenormalizer = CreateInterviewExportedCommentariesDenormalizer(interviewCommentariesStorage: interviewCommentariesStorage);

            interviewExportedCommentariesDenormalizer.Handle(Create.InterviewApprovedByHQEvent(interviewId: interviewId));

            Assert.That(interviewCommentaries.IsApprovedByHQ, Is.True);
            Assert.That(interviewCommentaries.Commentaries.Count, Is.EqualTo(0));
        }

        [Test]
        public void Handle_When_InterviewApprovedByHQ_event_arrived_with_not_empty_comment_Then_interview_should_be_marked_as_approved()
        {
            var interviewId = Guid.NewGuid();
            var comment = "comment";
            var interviewCommentaries = Create.InterviewCommentaries();
            var interviewCommentariesStorage = new TestInMemoryWriter<InterviewCommentaries>();
            interviewCommentariesStorage.Store(interviewCommentaries, interviewId);

            var interviewExportedCommentariesDenormalizer = CreateInterviewExportedCommentariesDenormalizer(interviewCommentariesStorage: interviewCommentariesStorage);

            interviewExportedCommentariesDenormalizer.Handle(Create.InterviewApprovedByHQEvent(interviewId: interviewId, comment:comment));

            Assert.That(interviewCommentaries.IsApprovedByHQ, Is.True);
            Assert.That(interviewCommentaries.Commentaries[0].Comment, Is.EqualTo(comment));
        }
    }
}