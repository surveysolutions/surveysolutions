using System;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.DashboardDenormalizerTests
{
    public class when_interviewer_assigned_event_has_no_interviewer_id
    {
        [Test]
        public void should_not_reset_userid_in_view()
        {
            var supervisorId = Id.gB;

            var dashboardItem = Create.Entity.InterviewView(interviewId: Id.g1);
            dashboardItem.ResponsibleId = supervisorId;

            var interviewViewStorage = new Mock<IPlainStorage<InterviewView>>();
            interviewViewStorage.Setup(x => x.GetById(It.IsAny<string>()))
                .Returns(dashboardItem);

            var denormalizer = Create.Service.DashboardDenormalizer(interviewViewRepository: interviewViewStorage.Object);

            var assignTime = DateTime.Now;
            var interviewerAssigned = Create.Event.InterviewerAssigned(Id.gA, null, assignTime);

            // Act 
            denormalizer.Handle(interviewerAssigned.ToPublishedEvent(eventSourceId: dashboardItem.InterviewId));

            // Assert
            Assert.That(dashboardItem.ResponsibleId, Is.EqualTo(supervisorId));
            Assert.That(dashboardItem.InterviewerAssignedDateTime, Is.EqualTo(assignTime));
            interviewViewStorage.Verify(x => x.Store(dashboardItem));
        }
    }
}
