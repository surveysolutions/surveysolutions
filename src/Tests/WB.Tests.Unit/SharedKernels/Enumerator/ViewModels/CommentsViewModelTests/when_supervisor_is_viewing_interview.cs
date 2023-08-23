using System;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.CommentsViewModelTests
{
    internal class when_supervisor_is_viewing_interview : CommentsViewModelTestsContext
    {
        private StatefulInterview interview;
        public Guid interviewId = Id.g1;
        private Guid questionId = Id.g2;
        private Guid interviewerId = Id.gA;
        private Guid supervisorId = Id.gB;
        private Guid hqId = Id.gC;
        private IPrincipal principal;

        [SetUp]
        public void SetUp()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: Create.Entity.TextQuestion(questionId: questionId));
            interview = Create.AggregateRoot.StatefulInterview(
                interviewId,
                userId: interviewerId,
                supervisorId: supervisorId,
                questionnaire: questionnaire);
            principal = Create.Other.SupervisorPrincipal(userId: supervisorId);
        }

        [Test]
        public void should_not_see_resolve_button_for_empty_comments()
        {
            var viewModel = CreateCommentsViewModel(interview, principal);
            viewModel.Init(interview.Id.FormatGuid(), Create.Identity(questionId));

            Assert.That(viewModel, Has.Property(nameof(CommentsViewModel.ResolveCommentsButtonVisible)).False);
        }

        [Test]
        public void should_resolve_button_be_available_when_unresolved_comment_exists()
        {
            interview.CommentAnswer(interviewerId, questionId, RosterVector.Empty, DateTimeOffset.Now, "IN comment");

            var viewModel = CreateCommentsViewModel(interview, principal);
            viewModel.Init(interview.Id.FormatGuid(), Create.Identity(questionId));

            Assert.That(viewModel, Has.Property(nameof(CommentsViewModel.ResolveCommentsButtonVisible)).True);
        }

        [Test]
        public void should_not_show_resolved_comments()
        {
            interview.CommentAnswer(interviewerId, questionId, RosterVector.Empty, DateTimeOffset.Now, "IN comment");
            interview.ResolveComment(Create.Command.ResolveCommentAnswer(entityId: Create.Identity(questionId)));

            var viewModel = CreateCommentsViewModel(interview, principal);
            viewModel.Init(interview.Id.FormatGuid(), Create.Identity(questionId));

            Assert.That(viewModel.Comments, Is.Empty);
            Assert.That(viewModel, Has.Property(nameof(CommentsViewModel.ResolveCommentsButtonVisible)).False, 
                () => "when all comments resolved should not show resolve button");
        }

        [Test]
        public void should_be_able_to_show_resolved_comments()
        {
            interview.CommentAnswer(interviewerId, questionId, RosterVector.Empty, DateTimeOffset.Now, "IN comment");
            interview.ResolveComment(Create.Command.ResolveCommentAnswer(entityId: Create.Identity(questionId)));

            var viewModel = CreateCommentsViewModel(interview, principal);
            viewModel.Init(interview.Id.FormatGuid(), Create.Identity(questionId));
            viewModel.ToggleShowResolvedComments.Execute();

            Assert.That(viewModel.Comments, Has.Count.EqualTo(1));
        }

        [Test]
        public void should_not_be_able_to_resolve_HQ_comments()
        {
            interview.CommentAnswer(hqId, questionId, RosterVector.Empty, DateTimeOffset.Now, "HQ comment");

            var viewModel = CreateCommentsViewModel(interview, principal);

            // Act
            viewModel.Init(interview.Id.FormatGuid(), Create.Identity(questionId));

            // Assert
            Assert.That(viewModel, Has.Property(nameof(CommentsViewModel.ResolveCommentsButtonVisible)).EqualTo(false));
        }
    }
}
