#nullable enable
using System;
using HotChocolate;
using NodaTime;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.CalendarEvents;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent;

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
            if (calendarEventToDelete == null) 
                if (calendarEventToDelete == null) 
                    throw new GraphQLException(new []{ 
                        ErrorBuilder.New()
                            .SetMessage("Calendar event was not found")
                            .SetCode(GraphQLErrorCodes.EntityNotFound)
                            .Build()});

            //check permissions
            if (calendarEventToDelete.InterviewId != null)
            {
                var interview = interviewSummaries.GetById(calendarEventToDelete.InterviewId.Value);
                if (authorizedUser.IsInterviewer && interview.ResponsibleId != authorizedUser.Id)
                    throw new GraphQLException(new []{ 
                        ErrorBuilder.New()
                            .SetMessage("User has no permissions")
                            .SetCode(ErrorCodes.Authentication.NotAuthorized)
                            .Build()});
            }
            else
            {
                var assignment = assignments.GetAssignment(calendarEventToDelete.AssignmentId);
                if (authorizedUser.IsInterviewer && assignment.ResponsibleId != authorizedUser.Id)
                    throw new GraphQLException(new []{ 
                        ErrorBuilder.New()
                            .SetMessage("User has no permissions")
                            .SetCode(ErrorCodes.Authentication.NotAuthorized)
                            .Build()});
            }
            
            commandService.Execute(new DeleteCalendarEventCommand(
                publicKey, authorizedUser.Id));

            return calendarEventToDelete;
        }

        public CalendarEvent? AddAssignmentCalendarEvent(int assignmentId, DateTimeOffset newStart, 
            string comment, string startTimezone,
            [Service] IAssignmentsService assignments,
            [Service] IAuthorizedUser authorizedUser,
            [Service] ICalendarEventService calendarEventService,
            [Service] ICommandService commandService)
        {
            if(DateTimeZoneProviders.Tzdb.GetZoneOrNull(startTimezone) == null)
                throw new GraphQLException(new []{ 
                    ErrorBuilder.New()
                        .SetMessage("Calendar event timezone has wrong format")
                        .SetCode(GraphQLErrorCodes.InvalidFormat)
                        .Build()});

            var assignment = assignments.GetAssignment(assignmentId);
            if(assignment == null)
                throw new GraphQLException(new []{ 
                    ErrorBuilder.New()
                        .SetMessage("Assignment was not found")
                        .SetCode(GraphQLErrorCodes.EntityNotFound)
                        .Build()});

            //check permissions
            if (authorizedUser.IsInterviewer && assignment.ResponsibleId != authorizedUser.Id)
                throw new GraphQLException(new []{ 
                        ErrorBuilder.New()
                            .SetMessage("User has no permissions")
                            .SetCode(ErrorCodes.Authentication.NotAuthorized)
                            .Build()});

            if(calendarEventService.GetActiveCalendarEventForAssignmentId(assignmentId) != null)
                throw new GraphQLException(new []{ 
                    ErrorBuilder.New()
                        .SetMessage("Assignment has active calendar event.")
                        .SetCode(GraphQLErrorCodes.EntityAlreadyExists)
                        .Build()});

            var publicKey = Guid.NewGuid();
            commandService.Execute(new CreateCalendarEventCommand(
                publicKey, 
                authorizedUser.Id, 
                newStart,
                startTimezone,
                null,
                null,
                assignmentId,
                comment));

            return calendarEventService.GetCalendarEventById(publicKey);
        }

        public CalendarEvent? AddInterviewCalendarEvent(Guid interviewId,
            DateTimeOffset newStart, string comment, string startTimezone,
            [Service] IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries,
            [Service] IAuthorizedUser authorizedUser,
            [Service] ICalendarEventService calendarEventService,
            [Service] ICommandService commandService)
        {
            /*if(string.IsNullOrEmpty(interviewKey))
                throw new ArgumentException("Interview key was not provided");*/

            if(DateTimeZoneProviders.Tzdb.GetZoneOrNull(startTimezone) == null)
                throw new GraphQLException(new []{ 
                    ErrorBuilder.New()
                        .SetMessage("Calendar event timezone has wrong format")
                        .SetCode(GraphQLErrorCodes.InvalidFormat)
                        .Build()});

            var interview = interviewSummaries.GetById(interviewId);
            if(interview == null)
                throw new GraphQLException(new []{ 
                    ErrorBuilder.New()
                        .SetMessage("Interview was not found")
                        .SetCode(GraphQLErrorCodes.EntityNotFound)
                        .Build()});

            //check permissions
            if(authorizedUser.IsInterviewer && interview.ResponsibleId != authorizedUser.Id) 
                throw new GraphQLException(new []{ 
                        ErrorBuilder.New()
                            .SetMessage("User has no permissions")
                            .SetCode(ErrorCodes.Authentication.NotAuthorized)
                            .Build()});

            if(calendarEventService.GetActiveCalendarEventForInterviewId(interviewId) != null)
                throw new GraphQLException(new []{ 
                    ErrorBuilder.New()
                        .SetMessage("Interview has active calendar event.")
                        .SetCode(GraphQLErrorCodes.EntityAlreadyExists)
                        .Build()});

            var newId = Guid.NewGuid();
            commandService.Execute(new CreateCalendarEventCommand(
                    newId, 
                    authorizedUser.Id, 
                    newStart,
                    startTimezone,
                    interviewId,
                    interview.Key,
                    interview.AssignmentId ?? 0,
                    comment));
            return calendarEventService.GetCalendarEventById(newId);
        }

        public CalendarEvent? UpdateCalendarEvent(Guid publicKey, DateTimeOffset newStart, 
            string comment, string startTimezone,
            [Service] IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries,
            [Service] IAssignmentsService assignments,
            [Service] IAuthorizedUser authorizedUser,
            [Service] ICalendarEventService calendarEventService,
            [Service] ICommandService commandService)
        {
            var calendarEventToUpdate = calendarEventService.GetCalendarEventById(publicKey);
            if (calendarEventToUpdate == null) 
                throw new GraphQLException(new []{ 
                    ErrorBuilder.New()
                        .SetMessage("Calendar event was not found")
                        .SetCode(GraphQLErrorCodes.EntityNotFound)
                        .Build()});

            if(DateTimeZoneProviders.Tzdb.GetZoneOrNull(startTimezone) == null)
                throw new GraphQLException(new []{ 
                    ErrorBuilder.New()
                        .SetMessage("Calendar event timezone has wrong format")
                        .SetCode(GraphQLErrorCodes.InvalidFormat)
                        .Build()});

            //check permissions
            if (calendarEventToUpdate.InterviewId != null)
            {
                var interview = interviewSummaries.GetById(calendarEventToUpdate.InterviewId.Value);
                if (authorizedUser.IsInterviewer && interview.ResponsibleId != authorizedUser.Id)
                    throw new GraphQLException(new []{ 
                        ErrorBuilder.New()
                            .SetMessage("User has no permissions")
                            .SetCode(ErrorCodes.Authentication.NotAuthorized)
                            .Build()});
            }
            else
            {
                var assignment = assignments.GetAssignment(calendarEventToUpdate.AssignmentId);
                if (authorizedUser.IsInterviewer && assignment.ResponsibleId != authorizedUser.Id)
                    throw new GraphQLException(new []{ 
                        ErrorBuilder.New()
                            .SetMessage("User has no permissions")
                            .SetCode(ErrorCodes.Authentication.NotAuthorized)
                            .Build()});
            }

            commandService.Execute(new UpdateCalendarEventCommand(
                    publicKey, 
                    authorizedUser.Id, 
                    newStart,
                    startTimezone,
                    comment));
                
            return calendarEventService.GetCalendarEventById(publicKey);
        }
    }
}
