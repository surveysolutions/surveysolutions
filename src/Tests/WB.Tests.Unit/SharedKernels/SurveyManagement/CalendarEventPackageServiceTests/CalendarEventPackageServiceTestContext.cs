using Microsoft.Extensions.Logging;
using Moq;
using Ncqrs.Eventing;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.CalendarEvents;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.CalendarEventPackageServiceTests
{
    [NUnit.Framework.TestOf(typeof(CalendarEventPackageService))]
    public class CalendarEventPackageServiceTestContext
    {
        protected Mock<ICommandService> ProcessPackage(CalendarEventPackage calendarEventPackage,
            CalendarEvent localCalendarEvent)
        {
            var commandService = new Mock<ICommandService>();
            var calendarEventService = new Mock<ICalendarEventService>();
            if (localCalendarEvent != null)
            {
                calendarEventService.Setup(s => s.GetCalendarEventById(localCalendarEvent.PublicKey))
                    .Returns(localCalendarEvent);

                if (localCalendarEvent.DeletedAtUtc == null && localCalendarEvent.CompletedAtUtc == null)
                {
                    if (localCalendarEvent.InterviewId.HasValue)
                        calendarEventService.Setup(s => s.GetActiveCalendarEventForInterviewId(localCalendarEvent.InterviewId.Value))
                            .Returns(localCalendarEvent);
                    else
                        calendarEventService.Setup(s => s.GetActiveCalendarEventForAssignmentId(localCalendarEvent.AssignmentId))
                            .Returns(localCalendarEvent);
                }
            }

            CommittedEvent[] res = new CommittedEvent[0];
            var serializer = Mock.Of<ISerializer>(s => s.Deserialize<CommittedEvent[]>(It.IsAny<string>()) == res);
            
            var service = Create.Service.CalendarEventPackageService(
                calendarEventService.Object,
                commandService.Object,
                Mock.Of<IQueryableReadSideRepositoryReader<InterviewSummary>>(),
                Mock.Of<IAssignmentsService>(),
                serializer);
            
            service.ProcessPackage(calendarEventPackage);

            return commandService;
        }

        protected static void Verify(Mock<ICommandService> commandService, CalendarEventPackage package, 
            bool restoreCalendarEventBefore = false, 
            bool restoreCalendarEventAfter = false, 
            bool deleteCalendarEventAfter = false)
        {
            commandService.Verify(c => c.Execute(
                It.Is<SyncCalendarEventEventsCommand>(c =>
                    c.PublicKey == package.CalendarEventId
                    && c.UserId == package.ResponsibleId
                    && c.RestoreCalendarEventBefore == restoreCalendarEventBefore
                    && c.RestoreCalendarEventAfter == restoreCalendarEventAfter
                    && c.DeleteCalendarEventAfter == deleteCalendarEventAfter
                    ),
                null), Times.Once);
        }
    }
}
