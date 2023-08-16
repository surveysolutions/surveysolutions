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
    internal class when_interviewer_viewing_interview : CommentsViewModelTestsContext
    {
        private StatefulInterview interview;
        public Guid interviewId = Id.g1;
        private Guid questionId = Id.g2;
        private Guid interviewerId = Id.gA;
        private Guid supervisorId = Id.gB;
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
            principal = Create.Other.InterviewerPrincipal(userId: interviewerId);
        }

        [Test]
        public void should_not_be_able_to_resolve_comments()
        {
            interview.CommentAnswer(interviewerId, questionId, RosterVector.Empty, DateTimeOffset.Now, "IN comment");

            var viewModel = CreateCommentsViewModel(interview, principal);
            viewModel.Init(interview.Id.FormatGuid(), Create.Identity(questionId));

            Assert.That(viewModel, Has.Property(nameof(CommentsViewModel.ResolveCommentsButtonVisible)).False);
        }
    }
}
