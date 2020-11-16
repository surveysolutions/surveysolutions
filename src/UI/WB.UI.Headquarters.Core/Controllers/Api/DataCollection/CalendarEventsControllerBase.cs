using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.CalendarEvents;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Infrastructure.Native.Storage;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection
{
    [RequestSizeLimit(1 * 10 * 1024 * 1024)]
    public abstract class CalendarEventsControllerBase: ControllerBase
    {
        protected readonly IHeadquartersEventStore eventStore;
        protected readonly ICalendarEventPackageService packageService;
        protected readonly ICalendarEventService calendarEventService;
        protected readonly IAuthorizedUser authorizedUser;

        protected CalendarEventsControllerBase(IHeadquartersEventStore eventStore,
            ICalendarEventPackageService packageService, 
            ICalendarEventService calendarEventService, IAuthorizedUser authorizedUser)
        {
            this.eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            this.packageService = packageService;
            this.calendarEventService = calendarEventService;
            this.authorizedUser = authorizedUser;
        }

        protected IActionResult PostPackage(CalendarEventPackageApiView package)
        {
            if (string.IsNullOrEmpty(package.Events))
                return BadRequest("Server cannot accept empty package content.");
            
            this.packageService.ProcessPackage(
                new CalendarEventPackage()
                {
                    CalendarEventId = package.CalendarEventId,
                    Events = package.Events,
                    IncomingDate = DateTime.UtcNow,
                    ResponsibleId = package.MetaInfo.ResponsibleId,
                    InterviewId = package.MetaInfo.InterviewId,
                    AssignmentId = package.MetaInfo.AssignmentId,
                    LastUpdateDate = package.MetaInfo.LastUpdateDateTime 
                });
            
            return Ok();
        }
        
        protected IActionResult GetCalendarEvent(Guid id)
        {
            var allEvents = eventStore.Read(id, 0).ToList();
            
            return new JsonResult(allEvents, 
                EventSerializerSettings.SyncronizationJsonSerializerSettings);
        }

        public abstract ActionResult<List<CalendarEventApiView>> Get();
    }
}
