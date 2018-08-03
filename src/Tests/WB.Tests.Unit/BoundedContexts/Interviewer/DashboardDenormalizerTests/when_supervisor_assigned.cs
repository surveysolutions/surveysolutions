using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.DashboardDenormalizerTests
{
    internal class when_supervisor_assigned : InterviewDashboardEventHandlerTest
    {
        [Test]
        public void should_change_responsible()
        {
            var supervisorId = Id.gB;

            var dashboardItem = Create.Entity.InterviewView(interviewId: Id.g1);
            var interviewViewStorage = new Mock<IPlainStorage<InterviewView>>();
            interviewViewStorage.Setup(x => x.GetById(It.IsAny<string>()))
                .Returns(dashboardItem);

            var denormalizer = Create.Service.DashboardDenormalizer(interviewViewRepository: interviewViewStorage.Object);

            // Act 
            denormalizer.Handle(Create.Event.SupervisorAssigned(Id.gA, supervisorId).ToPublishedEvent(eventSourceId: dashboardItem.InterviewId));

            // Assert
            Assert.That(dashboardItem.ResponsibleId, Is.EqualTo(supervisorId));
            interviewViewStorage.Verify(x => x.Store(dashboardItem));
        }
    }
}
