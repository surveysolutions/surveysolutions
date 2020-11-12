using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.CalendarEvents;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Interviewer.v3
{
    
    [Authorize(Roles = "Interviewer")]
    [Route("api/interviewer/v3/calendarevents")]
    public class CalendarEventsApiV3Controller : CalendarEventsControllerBase
    {
        public CalendarEventsApiV3Controller(IHeadquartersEventStore eventStore, 
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
        public override ActionResult<List<CalendarEventApiView>> Get() => base.Get();
    }
}
