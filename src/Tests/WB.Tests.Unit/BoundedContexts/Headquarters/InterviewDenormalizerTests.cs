using System;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters
{
    [TestOf(typeof(InterviewDenormalizer))]
    internal class InterviewDenormalizerTests
    {
        private readonly Guid interviewId = Id.g2;
        private InterviewSummary interviewSummary;
        private Mock<IInterviewFactory> mockOfInterviewFactory;
        private Mock<IQueryableReadSideRepositoryReader<InterviewSummary>> mockOfSummaryRepo;
        private Guid questionnaireId = Id.g1;
        private readonly long questionnaireVersion = 1515L;

        [SetUp]
        public void SetupMocks()
        {
            interviewSummary = Create.Entity.InterviewSummary(interviewId, questionnaireId, questionnaireVersion);
            mockOfSummaryRepo = new Mock<IQueryableReadSideRepositoryReader<InterviewSummary>>();
            mockOfSummaryRepo.Setup(summ => summ.GetById(Id.g2.FormatGuid())).Returns(interviewSummary);
            mockOfInterviewFactory = new Mock<IInterviewFactory>();
        }

        [Test]
        public void when_remove_rosters_should_be_questionnaire_identity_getting_from_inner_cache()
        {
            //arrange
            var interviewDenormalizer =
                new InterviewDenormalizer(mockOfInterviewFactory.Object, mockOfSummaryRepo.Object);

            interviewDenormalizer.Handle(Create.PublishedEvent.InterviewCreated(interviewId,
                questionnaireId: questionnaireId.ToString(), questionnaireVersion: questionnaireVersion));

            //act
            interviewDenormalizer.Handle(Create.PublishedEvent.RosterInstancesRemoved(interviewId));

            //assert
            mockOfInterviewFactory.Verify(x => x.RemoveRosters(
                It.Is<QuestionnaireIdentity>(y => y.QuestionnaireId == questionnaireId &&
                                                  y.Version == questionnaireVersion), interviewId,
                It.IsAny<Identity[]>()), Times.Once);

            mockOfSummaryRepo.Verify(ms => ms.GetById(Id.g2.FormatGuid()), Times.Never);
        }

        [Test]
        public void
            when_remove_rosters_then_questionnaire_identity_should_initialize_from_interview_summary_and_fill_inner_cache()
        {
            //arrange
            var interviewDenormalizer =
                new InterviewDenormalizer(mockOfInterviewFactory.Object, mockOfSummaryRepo.Object);

            //act
            interviewDenormalizer.Handle(Create.PublishedEvent.RosterInstancesRemoved(interviewId));

            //assert
            mockOfInterviewFactory.Verify(x => x.RemoveRosters(
                It.Is<QuestionnaireIdentity>(y => y.QuestionnaireId == questionnaireId &&
                                                  y.Version == questionnaireVersion), interviewId,
                It.IsAny<Identity[]>()), Times.Once);

            mockOfSummaryRepo.Verify(ms => ms.GetById(Id.g2.FormatGuid()), Times.Once);

            // ensure that next calls to RemoveRoster will not call summary repo for questionnaire id
            interviewDenormalizer.Handle(Create.PublishedEvent.RosterInstancesRemoved(interviewId));
            mockOfSummaryRepo.Verify(ms => ms.GetById(Id.g2.FormatGuid()), Times.Once);
        }
    }
}