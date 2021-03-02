#nullable enable
using System;
using System.Collections.Generic;
using HotChocolate;
using NodaTime;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.CalendarEvents;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.CalendarEvents
{
    public class CalendarEventResolver
    {
        public CalendarEvent? DeleteCalendarEvent(Guid publicKey, 
            [Service] IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries,
            [Service] IAssignmentsService assignments,
            [Service] IAuthorizedUser authorizedUser, 
            [Service] ICalendarEventService calendarEventService,
            [Service] ICommandService commandService)
        {
            var calendarEventToDelete = calendarEventService.GetCalendarEventById(publicKey);
            if (calendarEventToDelete == null) return calendarEventToDelete;

            //check permissions
            if (calendarEventToDelete.InterviewId != null)
            {
                var interview = interviewSummaries.GetById(calendarEventToDelete.InterviewId.Value);
                if (authorizedUser.IsInterviewer && interview.ResponsibleId != authorizedUser.Id)
                    throw new GraphQLException(new List<IError>(){ new Error("User has no permissions")});
            }
            else
            {
                var assignment = assignments.GetAssignment(calendarEventToDelete.AssignmentId);
                if (authorizedUser.IsInterviewer && assignment.ResponsibleId != authorizedUser.Id)
                    throw new GraphQLException(new List<IError>(){ new Error("User has no permissions")});
            }
            
            commandService.Execute(new DeleteCalendarEventCommand(
                publicKey, authorizedUser.Id));

            return calendarEventToDelete;
        }

        public CalendarEvent? AddOrUpdateCalendarEvent(Guid? publicKey, Guid? interviewId, 
            string? interviewKey, int assignmentId, DateTimeOffset newStart, 
            string comment, string startTimezone,
            [Service] IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries,
            [Service] IAssignmentsService assignments,
            [Service] IAuthorizedUser authorizedUser,
            [Service] ICalendarEventService calendarEventService,
            [Service] ICommandService commandService)
        {
            if(interviewId != null && string.IsNullOrEmpty(interviewKey))
                throw new ArgumentException("Interview key was not provided");

            if(interviewId == null && !string.IsNullOrEmpty(interviewKey))
                throw new ArgumentException("Interview Id was not provided");
            
            if(DateTimeZoneProviders.Tzdb.GetZoneOrNull(startTimezone) == null)
                throw new ArgumentException("Calendar event timezone has wrong format");

            //check permissions
            if (interviewId != null)
            {
                var interview = interviewSummaries.GetById(interviewId.Value);
                if (authorizedUser.IsInterviewer && interview.ResponsibleId != authorizedUser.Id)
                    throw new GraphQLException(new List<IError>(){ new Error("User has no permissions")});
            }
            else
            {
                var assignment = assignments.GetAssignment(assignmentId);
                if (authorizedUser.IsInterviewer && assignment.ResponsibleId != authorizedUser.Id)
                    throw new GraphQLException(new List<IError>(){ new Error("User has no permissions")});
            }

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
