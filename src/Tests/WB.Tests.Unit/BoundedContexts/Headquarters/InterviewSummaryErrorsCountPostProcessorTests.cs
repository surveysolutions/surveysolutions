using System;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters
{
    [TestFixture]
    public class InterviewSummaryErrorsCountPostProcessorTests
    {
        private InterviewSummaryErrorsCountPostProcessor subject;
        private Mock<IReadSideRepositoryWriter<InterviewSummary>> interviewSummaryRepo;
        private InterviewSummary summary;
        static readonly Guid interviewId = Id.g1;
        static readonly Guid interviewerQuestion = Id.g2;
        static readonly Guid prefilledQuestion = Id.g3;
        static readonly Guid supervisorQuestion = Id.g4;
        static readonly Guid hiddenQuestion = Id.g5;

        private StatefulInterview interview;


        [SetUp]
        public void Setup()
        {
            this.summary = Create.Entity.InterviewSummary(interviewId);

            this.interviewSummaryRepo = new Mock<IReadSideRepositoryWriter<InterviewSummary>>();
            this.interviewSummaryRepo
                .Setup(s => s.GetById(interviewId.FormatGuid()))
                .Returns(summary);
            
            this.subject = new InterviewSummaryErrorsCountPostProcessor(interviewSummaryRepo.Object);

            this.interview = Create.AggregateRoot.StatefulInterview(interviewId,
                questionnaire: Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextQuestion(interviewerQuestion),
                    Create.Entity.NumericQuestion(prefilledQuestion, prefilled: true),
                    Create.Entity.TextQuestion(supervisorQuestion, scope: QuestionScope.Supervisor, variable: "sup"),
                    Create.Entity.TextQuestion(hiddenQuestion, scope: QuestionScope.Hidden, variable: "hid")
            ));
        }

        [Test]
        public void should_store_errors_count_if_there_is_errors()
        {
            this.interview.ApplyEvent(Create.Event.AnswersDeclaredInvalid(
                Create.Identity(interviewerQuestion),
                Create.Identity(prefilledQuestion),
                Create.Identity(supervisorQuestion),
                Create.Identity(hiddenQuestion)));

            this.interview.ApplyEvent(new InterviewDeclaredInvalid());

            subject.Process(this.interview, null);

            this.interviewSummaryRepo.Verify(
                repo => repo.Store(It.Is<InterviewSummary>(s => s.ErrorsCount == 4), interviewId.FormatGuid()), Times.Once);
        }
        
        [Test]
        public void should_store_zero_errors_count_if_there_is_no_errors()
        {
            this.interview.ApplyEvent(new InterviewDeclaredValid());

            subject.Process(this.interview, null);

            this.interviewSummaryRepo.Verify(
                repo => repo.Store(It.Is<InterviewSummary>(s => s.ErrorsCount == 0), interviewId.FormatGuid()), Times.Once);
        }

        [Test]
        public void should_handle_non_existing_summary()
        {
            var someInterview = Create.AggregateRoot.StatefulInterview(Guid.NewGuid());

            Assert.DoesNotThrow(() => subject.Process(someInterview, new DeleteInterviewCommand(someInterview.Id, Guid.NewGuid())));

            this.interviewSummaryRepo.Verify(
                repo => repo.Store(It.IsAny<InterviewSummary>(), It.IsAny<string>()), Times.Never);
        }
    }
}
