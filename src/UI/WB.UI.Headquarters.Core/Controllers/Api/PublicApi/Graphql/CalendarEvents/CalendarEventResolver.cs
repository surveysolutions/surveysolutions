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
        private ICalendarEventService calendarEventService;
        private readonly IUnitOfWork unitOfWork;
        private readonly ICommandService commandService;

        public CalendarEventResolver(ICalendarEventService calendarEventService, IUnitOfWork unitOfWork, ICommandService commandService)
        {
            this.calendarEventService = calendarEventService;
            this.unitOfWork = unitOfWork;
            this.commandService = commandService;
        }

        public CalendarEvent? DeleteCalendarEvent(Guid publicKey, [Service]IAuthorizedUser authorizedUser)
        {
            var calendarEventToDelete = calendarEventService.GetCalendarEventById(publicKey);
            if(calendarEventToDelete != null)
                this.commandService.Execute(new DeleteCalendarEventCommand(
                    publicKey, 
                authorizedUser.Id));
            return calendarEventToDelete;
        }

        public CalendarEvent? AddOrUpdateCalendarEvent(Guid? publicKey, Guid? interviewId, 
            string? interviewKey, int assignmentId, DateTimeOffset newStart, 
            string comment, string startTimezone, [Service]IAuthorizedUser authorizedUser)
        {
            if(DateTimeZoneProviders.Tzdb.GetZoneOrNull(startTimezone) == null)
                throw new ArgumentException("Calendar event timezone has wrong format");

            if (publicKey == null)
            {
                var newId = Guid.NewGuid();
                this.commandService.Execute(new CreateCalendarEventCommand(
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
                this.commandService.Execute(new UpdateCalendarEventCommand(
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
