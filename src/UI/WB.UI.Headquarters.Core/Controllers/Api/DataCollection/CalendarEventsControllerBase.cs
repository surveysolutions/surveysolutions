using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection
{
    [RequestSizeLimit(1 * 10 * 1024 * 1024)]
    public abstract class CalendarEventsControllerBase: ControllerBase
    {
        private readonly IHeadquartersEventStore eventStore;
        private readonly ICalendarEventPackageService packageService;

        protected CalendarEventsControllerBase(IHeadquartersEventStore eventStore,
            ICalendarEventPackageService packageService)
        {
            this.eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            this.packageService = packageService;
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
            
            return new JsonResult(allEvents, Infrastructure.Native.Storage.EventSerializerSettings.SyncronizationJsonSerializerSettings);
        }

        public virtual ActionResult<List<CalendarEventApiView>> Get()
        {
            List<CalendarEventApiView> interviewApiViews = new List<CalendarEventApiView>(); 
                /*GetAllCalendarEventsForResponsible(this.authorizedUser.Id)
                .Select(interview => new CalendarEventApiView
                {
                }).ToList();*/

            return interviewApiViews;
        }
    }
}
