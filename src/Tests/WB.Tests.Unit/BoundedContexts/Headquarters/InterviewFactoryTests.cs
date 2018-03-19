using System;
using AutoFixture;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters
{
    [TestOf(typeof(InterviewFactory))]
    internal class InterviewFactoryFlagsTests
    {
        private Fixture fixture;
        private Guid interviewId;
        private Identity questionIdentity;

        [SetUp]
        public void Setup()
        {
            this.fixture = Create.Other.AutoFixture();

            var interviewSummaryRepository = Create.Storage.InMemoryReadeSideStorage<InterviewSummary>();
            fixture.Register<IQueryableReadSideRepositoryReader<InterviewSummary>>(() => interviewSummaryRepository);

            //arrange
            this.interviewId = Id.g1;
            this.questionIdentity = Identity.Create(Id.g2, Create.RosterVector(1));

            interviewSummaryRepository.Store(new InterviewSummary
            {
                SummaryId = interviewId.FormatGuid(),
                InterviewId = interviewId,
                Status = InterviewStatus.RejectedBySupervisor,
                ReceivedByInterviewer = true
            }, interviewId.FormatGuid());
        }

        [Test]
        public void when_remove_flag_question_received_by_interviewer()
        {
            var factory = fixture.Create<InterviewFactory>();

            //act
            var exception = Assert.Catch<InterviewException>(() => factory.SetFlagToQuestion(interviewId, questionIdentity, false));
            
            //assert
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.Message, Is.EqualTo($"Can't modify Interview {interviewId} on server, because it received by interviewer."));
        }

        [Test]
        public void when_set_flag_question_received_by_interviewer()
        {
            var factory = fixture.Create<InterviewFactory>();

            //act
            var exception = Assert.Catch<InterviewException>(() => factory.SetFlagToQuestion(interviewId, questionIdentity, true));

            //assert
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.Message, Is.EqualTo($"Can't modify Interview {interviewId} on server, because it received by interviewer."));
        }
    }
}
