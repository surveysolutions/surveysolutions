using System;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters
{
    [TestOf(typeof(InterviewDenormalizer))]
    internal class InterviewDenormalizerTests
    {
        private InterviewDenormalizer CrateInterviewDenormalizer(IInterviewFactory interviewFactory = null,
            IQueryableReadSideRepositoryReader<InterviewSummary> summaries = null,
            IQuestionnaireStorage questionnaireStorage = null)
            => new InterviewDenormalizer(interviewFactory ?? Mock.Of<IInterviewFactory>(),
                summaries ?? Mock.Of<IQueryableReadSideRepositoryReader<InterviewSummary>>(),
                questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>());

        [Test]
        public void when_remove_rosters_should_be_questionnaire_identity_getting_from_inner_cache()
        {
            //arrange
            Guid interviewId = Id.g2;
            Guid questionnaireId = Id.g1;
            long questionnaireVersion = 1515L;
            var interviewSummary = Create.Entity.InterviewSummary(interviewId, questionnaireId, questionnaireVersion);
            var mockOfSummaryRepo = new Mock<IQueryableReadSideRepositoryReader<InterviewSummary>>();
            mockOfSummaryRepo.Setup(summ => summ.GetById(Id.g2.FormatGuid())).Returns(interviewSummary);
            var mockOfQuestionnaireStorage = new Mock<IQuestionnaireStorage>();

            var interviewDenormalizer = CrateInterviewDenormalizer(summaries: mockOfSummaryRepo.Object,
                questionnaireStorage: mockOfQuestionnaireStorage.Object);

            var state = Create.Entity.InterviewState(interviewId);

            interviewDenormalizer.Update(state, Create.PublishedEvent.InterviewCreated(interviewId,
                questionnaireId: questionnaireId.ToString(), questionnaireVersion: questionnaireVersion));

            //act
            interviewDenormalizer.Update(state, Create.PublishedEvent.RosterInstancesRemoved(interviewId));

            //assert
            mockOfQuestionnaireStorage.Verify(x => x.GetQuestionnaireDocument(It.Is<QuestionnaireIdentity>(y =>
                y.QuestionnaireId == questionnaireId && y.Version == questionnaireVersion)), Times.Once);

            mockOfSummaryRepo.Verify(ms => ms.GetById(Id.g2.FormatGuid()), Times.Never);
        }

        [Test]
        public void when_remove_rosters_then_questionnaire_identity_should_initialize_from_interview_summary_and_fill_inner_cache()
        {
            //arrange
            Guid interviewId = Id.g2;
            Guid questionnaireId = Id.g1;
            long questionnaireVersion = 1515L;
            var interviewSummary = Create.Entity.InterviewSummary(interviewId, questionnaireId, questionnaireVersion);
            var mockOfSummaryRepo = new Mock<IQueryableReadSideRepositoryReader<InterviewSummary>>();
            mockOfSummaryRepo.Setup(summ => summ.GetById(Id.g2.FormatGuid())).Returns(interviewSummary);
            var mockOfQuestionnaireStorage = new Mock<IQuestionnaireStorage>();

            var interviewDenormalizer = CrateInterviewDenormalizer(summaries: mockOfSummaryRepo.Object,
                questionnaireStorage: mockOfQuestionnaireStorage.Object);
            
            var state = Create.Entity.InterviewState(interviewId);

            //act
            interviewDenormalizer.Update(state, Create.PublishedEvent.RosterInstancesRemoved(interviewId));

            //assert
            mockOfQuestionnaireStorage.Verify(x => x.GetQuestionnaireDocument(It.Is<QuestionnaireIdentity>(y =>
                y.QuestionnaireId == questionnaireId && y.Version == questionnaireVersion)), Times.Once);
            mockOfSummaryRepo.Verify(ms => ms.GetById(Id.g2.FormatGuid()), Times.Once);
        }

        [Test]
        public void when_remove_rosters_and_questionnaire_id_in_cache_then_questionnaire_identity_should_not_initialize_from_interview_summary()
        {
            Guid interviewId = Id.g2;
            Guid questionnaireId = Id.g1;
            long questionnaireVersion = 1515L;
            var interviewSummary = Create.Entity.InterviewSummary(interviewId, questionnaireId, questionnaireVersion);
            var mockOfSummaryRepo = new Mock<IQueryableReadSideRepositoryReader<InterviewSummary>>();
            mockOfSummaryRepo.Setup(summ => summ.GetById(Id.g2.FormatGuid())).Returns(interviewSummary);
            var mockOfInterviewFactory = new Mock<IInterviewFactory>();

            //arrange
            var interviewDenormalizer = CrateInterviewDenormalizer(mockOfInterviewFactory.Object, mockOfSummaryRepo.Object);

            var state = Create.Entity.InterviewState(interviewId);
            interviewDenormalizer.Update(state, Create.PublishedEvent.RosterInstancesRemoved(interviewId));

            //act
            interviewDenormalizer.Update(state, Create.PublishedEvent.RosterInstancesRemoved(interviewId));

            //assert
            mockOfSummaryRepo.Verify(ms => ms.GetById(Id.g2.FormatGuid()), Times.Once);
        }
    }
}