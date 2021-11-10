using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.CalendarEvents;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.Domain;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Storage;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection
{
    [RequestSizeLimit(1 * 10 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
    public abstract class CalendarEventsControllerBase: ControllerBase
    {
        protected readonly IHeadquartersEventStore eventStore;
        protected readonly ICalendarEventService calendarEventService;
        protected readonly IAuthorizedUser authorizedUser;
        private readonly ILogger<CalendarEventsControllerBase> logger;
        private readonly IInScopeExecutor inScopeExecutor;

        protected CalendarEventsControllerBase(IHeadquartersEventStore eventStore,
            ICalendarEventService calendarEventService, 
            IAuthorizedUser authorizedUser,
            ILogger<CalendarEventsControllerBase> logger,
            IInScopeExecutor inScopeExecutor)
        {
            this.eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            this.calendarEventService = calendarEventService;
            this.authorizedUser = authorizedUser;
            this.logger = logger;
            this.inScopeExecutor = inScopeExecutor;
        }

        protected IActionResult PostPackage(CalendarEventPackageApiView package)
        {
            if (string.IsNullOrEmpty(package.Events))
                return BadRequest("Server cannot accept empty package content.");

            var calendarEventPackage = new CalendarEventPackage()
            {
                CalendarEventId = package.CalendarEventId,
                Events = package.Events,
                ResponsibleId = authorizedUser.Id,
                InterviewId = package.MetaInfo.InterviewId,
                AssignmentId = package.MetaInfo.AssignmentId,
                LastUpdateDateUtc = package.MetaInfo.LastUpdateDateTime.UtcDateTime,
                IsDeleted = package.MetaInfo.IsDeleted,
            };

            try
            {
                inScopeExecutor.Execute(serviceLocator =>
                {
                    var calendarEventPackageService = serviceLocator.GetInstance<ICalendarEventPackageService>();
                    calendarEventPackageService.ProcessPackage(calendarEventPackage);
                });
            }
            catch (Exception exception)
            {
                this.logger.LogError(
                    exception,
                    $"Calendar event events by {calendarEventPackage.CalendarEventId} processing failed. Reason: '{exception.Message}'");
                throw;
            }

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
