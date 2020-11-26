using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Interviewer")]
    public class CalendarEventsController : Controller
    {
        private readonly ICommandService commandService;
        private readonly IAuthorizedUser authorizedUser;
        
        public CalendarEventsController(ICommandService commandService, IAuthorizedUser authorizedUser)
        {
            this.commandService = commandService;
            this.authorizedUser = authorizedUser;
        }

        [HttpPost]
        public IActionResult UpdateCalendarEvent([FromBody] UpdateInterviewCalendarEventRequest request)
        {
            if (request.NewDate == null)
                return BadRequest("Calendar event has wrong format");

            var interviewId = Guid.TryParse(request.InterviewId, out Guid passedInterviewId)
                ? passedInterviewId
                : (Guid?) null;

            if(DateTimeZoneProviders.Tzdb.GetZoneOrNull(request.Timezone) == null)
                return BadRequest("Calendar event timezone has wrong format");

            if (request.Id == null)
            {
                this.commandService.Execute(new CreateCalendarEventCommand(
                    Guid.NewGuid(), 
                    this.authorizedUser.Id, 
                    request.NewDate.Value,
                    request.Timezone,
                    interviewId,
                    request.InterviewKey,
                    request.AssignmentId,
                    request.Comment));
            }
            else
            {
                this.commandService.Execute(new UpdateCalendarEventCommand(
                    request.Id.Value, 
                    this.authorizedUser.Id, 
                    request.NewDate.Value,
                    request.Timezone,
                    request.Comment));
            }
            return this.Content("ok");
        }
        
        [HttpDelete]
        public IActionResult DeleteCalendarEvent(Guid id)
        {
            this.commandService.Execute(new DeleteCalendarEventCommand(
                id, 
                this.authorizedUser.Id));
            
            return this.Content("ok");
        }
    }
}
