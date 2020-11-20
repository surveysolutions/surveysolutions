using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.CalendarEvents;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Supervisor.v1
{
    [Authorize(Roles = "Supervisor")]
    [Route("api/supervisor/v1/calendarevents")]
    public class CalendarEventsApiV1Controller: CalendarEventsControllerBase
    {
        public CalendarEventsApiV1Controller(IHeadquartersEventStore eventStore, 
            ICalendarEventPackageService packageService, ICalendarEventService calendarEventService, IAuthorizedUser authorizedUser) 
            : base(eventStore, packageService, calendarEventService, authorizedUser)
        {
        }
        
        [HttpPost]
        [Route("{id:guid}")]
        [WriteToSyncLog(SynchronizationLogType.PostCalendarEvent)]
        public IActionResult Post([FromBody]CalendarEventPackageApiView package) => base.PostPackage(package);
        
        [HttpGet]
        [Route("{id:guid}")]
        [WriteToSyncLog(SynchronizationLogType.GetCalendarEvent)]
        public IActionResult Details(Guid id) => base.GetCalendarEvent(id);
        
        [HttpGet]
        [Route("")]
        [WriteToSyncLog(SynchronizationLogType.GetCalendarEvents)]
        public override ActionResult<List<CalendarEventApiView>> Get()
        {
            var interviewApiViews = 
                calendarEventService.GetAllCalendarEventsUnderSupervisor(this.authorizedUser.Id); 

            return interviewApiViews.Select(x => new CalendarEventApiView()
            {
                CalendarEventId = x.PublicKey,
                Sequence = eventStore.GetLastEventSequence(x.PublicKey),
                LastEventId = eventStore.GetLastEventId(x.PublicKey)
            }).ToList();
        }
    }
}
