using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    [TestOf(typeof(InterviewFactory))]
    internal class InterviewFactoryTests
    {
        private static InterviewFactory CreateInterviewFactory(
            IPlainStorageAccessor<InterviewDbEntity> interviewEntitiesRepository = null,
            IReadSideRepositoryReader<InterviewSummary> interviewSummaryRepository = null)
            => new InterviewFactory(
                interviewEntitiesRepository: interviewEntitiesRepository ??
                                             Mock.Of<IPlainStorageAccessor<InterviewDbEntity>>(),
                interviewSummaryRepository: interviewSummaryRepository ??
                                            Mock.Of<IReadSideRepositoryReader<InterviewSummary>>());
        [Test]
        public void when_remove_flag_question_received_by_interviewer()
        {
            //arrange
            //arrange
            var interviewId = Guid.Parse("11111111111111111111111111111111");
            var questionIdentity = Identity.Create(Guid.Parse("22222222222222222222222222222222"),
                Create.RosterVector(1));

            var interviewSummaryRepository = Create.Storage.InMemoryReadeSideStorage<InterviewSummary>();
            interviewSummaryRepository.Store(new InterviewSummary
            {
                SummaryId = interviewId.FormatGuid(),
                InterviewId = interviewId,
                Status = InterviewStatus.RejectedBySupervisor,
                ReceivedByInterviewer = true
            }, interviewId.FormatGuid());

            var factory = CreateInterviewFactory(interviewSummaryRepository: interviewSummaryRepository);

            //act
            var exception = Assert.Catch<InterviewException>(() => factory.RemoveFlagFromQuestion(interviewId, questionIdentity));
            //assert
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.Message, Is.EqualTo($"Can't modify Interview {interviewId} on server, because it received by interviewer."));
        }

        [Test]
        public void when_set_flag_question_received_by_interviewer()
        {
            //arrange
            var interviewId = Guid.Parse("11111111111111111111111111111111");
            var questionIdentity = Identity.Create(Guid.Parse("22222222222222222222222222222222"),
                Create.RosterVector(1));

            var interviewSummaryRepository = Create.Storage.InMemoryReadeSideStorage<InterviewSummary>();
            interviewSummaryRepository.Store(new InterviewSummary
            {
                SummaryId = interviewId.FormatGuid(),
                InterviewId = interviewId,
                Status = InterviewStatus.RejectedBySupervisor,
                ReceivedByInterviewer = true
            }, interviewId.FormatGuid());

            var factory = CreateInterviewFactory(interviewSummaryRepository: interviewSummaryRepository);

            //act
            var exception = Assert.Catch<InterviewException>(() => factory.SetFlagToQuestion(interviewId, questionIdentity));
            //assert
            Assert.That(exception, Is.Not.Null); 
            Assert.That(exception.Message, Is.EqualTo($"Can't modify Interview {interviewId} on server, because it received by interviewer."));
        }

        [Test]
        public void when_adding_flag_to_question()
        {
            //arrange
            var interviewId = Guid.Parse("11111111111111111111111111111111");
            var questionIdentity = Identity.Create(Guid.Parse("22222222222222222222222222222222"),
                Create.RosterVector(1));

            var interviewSummaryRepository = Create.Storage.InMemoryReadeSideStorage<InterviewSummary>();
            interviewSummaryRepository.Store(new InterviewSummary
            {
                SummaryId = interviewId.FormatGuid(),
                InterviewId = interviewId,
                Status = InterviewStatus.Completed,
                ReceivedByInterviewer = false
            }, interviewId.FormatGuid());

            var interviewQuestion = new InterviewDbEntity
            {
                InterviewId = interviewId,
                Identity = questionIdentity,
                HasFlag = false
            };

            var mockOfInterviewEntitiesRepository = new Mock<IPlainStorageAccessor<InterviewDbEntity>>();
            mockOfInterviewEntitiesRepository
                .Setup(x => x.Query(Moq.It.IsAny<Func<IQueryable<InterviewDbEntity>, InterviewDbEntity>>()))
                .Returns(interviewQuestion);

            var factory = CreateInterviewFactory(mockOfInterviewEntitiesRepository.Object, interviewSummaryRepository);

            //act
            factory.SetFlagToQuestion(interviewId, questionIdentity);

            //assert
            mockOfInterviewEntitiesRepository.Verify(
                x => x.Store(Moq.It.Is<InterviewDbEntity>(y => y.HasFlag == true), null), Times.Once);
        }

        [Test]
        public void when_removing_flag_from_question()
        {
            //arrange
            var interviewId = Guid.Parse("11111111111111111111111111111111");
            var questionIdentity = Identity.Create(Guid.Parse("22222222222222222222222222222222"),
                Create.RosterVector(1));

            var interviewQuestion = new InterviewDbEntity
            {
                InterviewId = interviewId,
                Identity = questionIdentity,
                HasFlag = true
            };

            var interviewSummaryRepository = Create.Storage.InMemoryReadeSideStorage<InterviewSummary>();
            interviewSummaryRepository.Store(new InterviewSummary
            {
                SummaryId = interviewId.FormatGuid(),
                InterviewId = interviewId,
                Status = InterviewStatus.Completed,
                ReceivedByInterviewer = false
            }, interviewId.FormatGuid());

            var mockOfInterviewEntitiesRepository = new Mock<IPlainStorageAccessor<InterviewDbEntity>>();
            mockOfInterviewEntitiesRepository
                .Setup(x => x.Query(Moq.It.IsAny<Func<IQueryable<InterviewDbEntity>, InterviewDbEntity>>()))
                .Returns(interviewQuestion);

            var factory = CreateInterviewFactory(mockOfInterviewEntitiesRepository.Object, interviewSummaryRepository);


            //act
            factory.RemoveFlagFromQuestion(interviewId, questionIdentity);
            
            //assert
            mockOfInterviewEntitiesRepository.Verify(
                x => x.Store(Moq.It.Is<InterviewDbEntity>(y => y.HasFlag == false), null), Times.Once);
        }
    }
}