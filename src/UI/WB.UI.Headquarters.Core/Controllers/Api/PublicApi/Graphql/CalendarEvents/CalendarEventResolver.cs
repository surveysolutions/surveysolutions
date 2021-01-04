#nullable enable
using System;
using HotChocolate;
using NodaTime;
using WB.Core.BoundedContexts.Headquarters.CalendarEvents;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.CalendarEvents
{
    public class CalendarEventResolver
    {
        public CalendarEvent? DeleteCalendarEvent(Guid publicKey, [Service]IAuthorizedUser authorizedUser, 
            [Service] ICalendarEventService calendarEventService,
            [Service] ICommandService commandService)
        {
            var calendarEventToDelete = calendarEventService.GetCalendarEventById(publicKey);
            if(calendarEventToDelete != null)
                commandService.Execute(new DeleteCalendarEventCommand(
                    publicKey, 
                authorizedUser.Id));
            return calendarEventToDelete;
        }

        public CalendarEvent? AddOrUpdateCalendarEvent(Guid? publicKey, Guid? interviewId, 
            string? interviewKey, int assignmentId, DateTimeOffset newStart, 
            string comment, string startTimezone, [Service] IAuthorizedUser authorizedUser,
            [Service] ICalendarEventService calendarEventService,
            [Service] ICommandService commandService)
        {
            if(interviewId != null && string.IsNullOrEmpty(interviewKey))
                throw new ArgumentException("Interview key was not provided");

            if(interviewId == null && !string.IsNullOrEmpty(interviewKey))
                throw new ArgumentException("Interview Id was not provided");
            
            if(DateTimeZoneProviders.Tzdb.GetZoneOrNull(startTimezone) == null)
                throw new ArgumentException("Calendar event timezone has wrong format");

            if (publicKey == null)
            {
                var newId = Guid.NewGuid();
                commandService.Execute(new CreateCalendarEventCommand(
                    newId, 
                    authorizedUser.Id, 
                    newStart,
                    startTimezone,
                    interviewId,
                    interviewKey,
                    assignmentId,
                    comment));
                return calendarEventService.GetCalendarEventById(newId);
            }
            else
            {
                commandService.Execute(new UpdateCalendarEventCommand(
                    publicKey.Value, 
                    authorizedUser.Id, 
                    newStart,
                    startTimezone,
                    comment));
                
                return calendarEventService.GetCalendarEventById(publicKey.Value);
            }
        }
    }
}
