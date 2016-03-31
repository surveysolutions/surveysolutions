using System;
using Moq;
using NSubstitute;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.Implementation.Repositories;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewRepositoryTests
{
    [TestFixture]
    internal class StatefulInterviewRepositoryNUnitTests
    {
        [Test]
        public void When_StatefullInterview_has_at_least_one_LinkedOptionsChanged_event_Then_Event_store_should_no_be_updated()
        {
            var liteEventBusMock = new Mock<ILiteEventBus>();

            var statefulInterview = Create.StatefulInterview(userId: null, questionnaire: null);
            statefulInterview.Apply(Create.Event.LinkedOptionsChanged());

            var statefulInterviewRepository = CreteStatefulInterviewRepository(statefulInterview,
                liteEventBusMock.Object);

            var result = statefulInterviewRepository.Get(Guid.NewGuid().FormatGuid());

            Assert.That(result, Is.EqualTo(statefulInterview));
            liteEventBusMock.Verify(x => x.CommitUncommittedEvents(statefulInterview, Moq.It.IsAny<string>()),
                Times.Never);
        }

        [Test]
        public void When_StatefullInterview_doesnt_have_at_least_one_LinkedOptionsChanged_event_Then_Event_store_should_no_be_updated()
        {
            var liteEventBusMock = new Mock<ILiteEventBus>();

            IQuestionnaire questionnaire = Substitute.For<IQuestionnaire>();

            var statefulInterview = Create.StatefulInterview(userId: null, questionnaire: questionnaire);

            var statefulInterviewRepository = CreteStatefulInterviewRepository(statefulInterview,
                liteEventBusMock.Object);

            var result = statefulInterviewRepository.Get(Guid.NewGuid().FormatGuid());

            Assert.That(result, Is.EqualTo(statefulInterview));
            Assert.That(result.HasLinkedOptionsChangedEvents, Is.True);
            liteEventBusMock.Verify(x => x.CommitUncommittedEvents(statefulInterview, Moq.It.IsAny<string>()),
                Times.Once);
        }

        private StatefulInterviewRepository CreteStatefulInterviewRepository(StatefulInterview statefulInterview = null, ILiteEventBus liteEventBus=null)
        {
            return
                new StatefulInterviewRepository(
                    Mock.Of<IEventSourcedAggregateRootRepository>(
                        _ => _.GetLatest(Moq.It.IsAny<Type>(), Moq.It.IsAny<Guid>()) == statefulInterview),
                    liteEventBus??Mock.Of<ILiteEventBus>());
        }
    }
}