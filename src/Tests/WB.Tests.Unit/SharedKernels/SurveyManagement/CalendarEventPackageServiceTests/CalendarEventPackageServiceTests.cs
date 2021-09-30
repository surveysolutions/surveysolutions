using System;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.CalendarEvents;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.CalendarEventPackageServiceTests
{
    public class CalendarEventPackageServiceTests : CalendarEventPackageServiceTestContext
    {
        [Test]
        public void when_received_new_calendar_event_should_precess_them()
        {
            var package = new CalendarEventPackage()
            {
                ResponsibleId = Id.g1,
                CalendarEventId = Id.g2,
                AssignmentId = 7,
                IsDeleted = false,
                LastUpdateDateUtc = DateTime.UtcNow
            };
            CalendarEvent local = null;
            
            var commandService = SetupServiceAndProcessPackage(package, local);
            
            Verify(commandService, package);
        }
        
        [Test]
        public void when_received_updated_calendar_event_should_precess_them()
        {
            var package = new CalendarEventPackage()
            {
                ResponsibleId = Id.g1,
                CalendarEventId = Id.g2,
                AssignmentId = 7,
                IsDeleted = false,
                LastUpdateDateUtc = DateTime.UtcNow.AddMinutes(5)
            };
            var local = Create.Entity.CalendarEvent(
                publicKey: Id.g2,
                assignmentId: 7,
                updateDateUtc: DateTime.UtcNow
            );
            
            var commandService = SetupServiceAndProcessPackage(package, local);
            
            Verify(commandService, package);
        }


        [Test]
        public void when_received_calendar_event_updated_on_server_should_precess_them()
        {
            var package = new CalendarEventPackage()
            {
                ResponsibleId = Id.g1,
                CalendarEventId = Id.g2,
                AssignmentId = 7,
                IsDeleted = false,
                LastUpdateDateUtc = DateTime.UtcNow
            };
            var local = Create.Entity.CalendarEvent(
                publicKey: Id.g2,
                assignmentId: 7,
                updateDateUtc: DateTime.UtcNow.AddMinutes(5)
            );
            
            var commandService = SetupServiceAndProcessPackage(package, local);
            
            Verify(commandService, package);
        }
        
        [Test]
        public void when_received_updated_deleted_calendar_event_should_precess_them()
        {
            var package = new CalendarEventPackage()
            {
                ResponsibleId = Id.g1,
                CalendarEventId = Id.g2,
                AssignmentId = 7,
                IsDeleted = true,
                LastUpdateDateUtc = DateTime.UtcNow.AddMinutes(5)
            };
            var local = Create.Entity.CalendarEvent(
                publicKey: Id.g2,
                assignmentId: 7,
                updateDateUtc: DateTime.UtcNow
            );
            
            var commandService = SetupServiceAndProcessPackage(package, local);
            
            Verify(commandService, package);
        }
        
        [Test]
        public void when_received_deleted_calendar_event_but_updated_on_server_should_precess_them()
        {
            var package = new CalendarEventPackage()
            {
                ResponsibleId = Id.g1,
                CalendarEventId = Id.g2,
                AssignmentId = 7,
                IsDeleted = true,
                LastUpdateDateUtc = DateTime.UtcNow
            };
            var local = Create.Entity.CalendarEvent(
                publicKey: Id.g2,
                assignmentId: 7,
                deletedAtUtc: null,
                updateDateUtc: DateTime.UtcNow.AddMinutes(5)
            );
            
            var commandService = SetupServiceAndProcessPackage(package, local);
            
            Verify(commandService, package, restoreCalendarEventAfter: true);
        }
        
        [Test]
        public void when_received_updated_calendar_event_but_on_server_it_is_deleted_should_precess_them()
        {
            var package = new CalendarEventPackage()
            {
                ResponsibleId = Id.g1,
                CalendarEventId = Id.g2,
                AssignmentId = 7,
                IsDeleted = false,
                LastUpdateDateUtc = DateTime.UtcNow.AddMinutes(5)
            };
            var local = Create.Entity.CalendarEvent(
                publicKey: Id.g2,
                assignmentId: 7,
                deletedAtUtc: DateTime.UtcNow,
                updateDateUtc: DateTime.UtcNow
            );
            
            var commandService = SetupServiceAndProcessPackage(package, local);
            
            Verify(commandService, package, restoreCalendarEventBefore: true);
        }
        
        [Test]
        public void when_received_calendar_event_but_updated_and_deleted_on_server_should_precess_them()
        {
            var package = new CalendarEventPackage()
            {
                ResponsibleId = Id.g1,
                CalendarEventId = Id.g2,
                AssignmentId = 7,
                IsDeleted = false,
                LastUpdateDateUtc = DateTime.UtcNow
            };
            var local = Create.Entity.CalendarEvent(
                publicKey: Id.g2,
                assignmentId: 7,
                deletedAtUtc: DateTime.UtcNow.AddMinutes(5),
                updateDateUtc: DateTime.UtcNow.AddMinutes(5)
            );

            var commandService = SetupServiceAndProcessPackage(package, local);
            
            Verify(commandService, package, restoreCalendarEventBefore: false);
        }
        
        [Test]
        public void when_receiving_package_having_chages_before_local()
        {
            var package = new CalendarEventPackage()
            {
                ResponsibleId = Id.g1,
                CalendarEventId = Id.g2,
                AssignmentId = 7,
                IsDeleted = false,
                LastUpdateDateUtc = DateTime.UtcNow
            };
            var local = Create.Entity.CalendarEvent(
                publicKey: Id.g2,
                assignmentId: 7,
                deletedAtUtc: null,
                updateDateUtc: DateTime.UtcNow.AddMinutes(5)
            );
            
            var commandService = SetupServiceAndProcessPackage(package, local);
            
            Verify(commandService, package, shouldRestorePreviousStateAfterApplying: true);
        }
    }
}
