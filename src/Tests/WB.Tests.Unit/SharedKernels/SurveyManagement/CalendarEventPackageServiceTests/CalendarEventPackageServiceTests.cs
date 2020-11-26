using System;
using Moq;
using NUnit.Framework;
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
            
            var commandService = ProcessPackage(package, local);
            
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
            var local = new CalendarEvent()
            {
                PublicKey = Id.g2,
                AssignmentId = 7,
                UpdateDateUtc = DateTime.UtcNow
            };
            
            var commandService = ProcessPackage(package, local);
            
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
            var local = new CalendarEvent()
            {
                PublicKey = Id.g2,
                AssignmentId = 7,
                UpdateDateUtc = DateTime.UtcNow.AddMinutes(5)
            };
            
            var commandService = ProcessPackage(package, local);
            
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
            var local = new CalendarEvent()
            {
                PublicKey = Id.g2,
                AssignmentId = 7,
                UpdateDateUtc = DateTime.UtcNow
            };
            
            var commandService = ProcessPackage(package, local);
            
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
            var local = new CalendarEvent()
            {
                PublicKey = Id.g2,
                AssignmentId = 7,
                DeletedAtUtc = null,
                UpdateDateUtc = DateTime.UtcNow.AddMinutes(5)
            };
            
            var commandService = ProcessPackage(package, local);
            
            Verify(commandService, package, shouldRestoreCalendar: true);
        }
        
        
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
            var local = new CalendarEvent()
            {
                PublicKey = Id.g2,
                AssignmentId = 7,
                DeletedAtUtc = DateTime.UtcNow,
                UpdateDateUtc = DateTime.UtcNow
            };
            
            var commandService = ProcessPackage(package, local);
            
            Verify(commandService, package, true);
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
            var local = new CalendarEvent()
            {
                PublicKey = Id.g2,
                AssignmentId = 7,
                DeletedAtUtc = DateTime.UtcNow.AddMinutes(5),
                UpdateDateUtc = DateTime.UtcNow.AddMinutes(5)
            };
            
            var commandService = ProcessPackage(package, local);
            
            Verify(commandService, package, shouldRestoreCalendar: false);
        }
    }
}
