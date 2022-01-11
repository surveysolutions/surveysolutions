using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.CalendarEvents;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Enumerator.Native.WebInterview;
using WB.UI.Headquarters.Code.CommandTransformation;
using WB.UI.Headquarters.Filters;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.UI.Headquarters.Controllers.Api
{
    [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
    [ApiValidationAntiForgeryToken]
    [Route("api/[controller]/[action]/{id?}")]
    public class CommandApiController : ControllerBase
    {
        public class JsonCommandResponse
        {
            public bool IsSuccess = false;

            public string DomainException { get; set; }
        }

        public class JsonBundleCommandResponse
        {
            public List<JsonCommandResponse> CommandStatuses { get; set; }
            public JsonBundleCommandResponse()
            {
                this.CommandStatuses = new List<JsonCommandResponse>();
            }
        }

        private readonly ICommandService commandService;
        private readonly ICommandDeserializer commandDeserializer;
        private readonly IInterviewFactory interviewFactory;
        private ICommandTransformator commandTransformator;
        private readonly ICalendarEventService calendarEventService;
        private readonly IAssignmentsService assignmentsStorage;

        public CommandApiController(
            ICommandService commandService, ICommandDeserializer commandDeserializer,
            IInterviewFactory interviewFactory, ICommandTransformator commandTransformator, 
            ICalendarEventService calendarEventService, IAssignmentsService assignmentsStorage)
        {
            this.commandService = commandService;
            this.commandDeserializer = commandDeserializer;
            this.interviewFactory = interviewFactory;
            this.commandTransformator = commandTransformator;
            this.calendarEventService = calendarEventService;
            this.assignmentsStorage = assignmentsStorage;
        }

        [HttpPost]
        [ObservingNotAllowed]
        public JsonBundleCommandResponse ExecuteCommands(JsonBundleCommandRequest request)
        {
            var response = new JsonBundleCommandResponse();

            if (request == null || string.IsNullOrEmpty(request.Type) || request.Commands == null)
                throw new NullReferenceException();

            foreach (var command in request.Commands)
            {
                response.CommandStatuses.Add(this.Execute(new JsonCommandRequest() { Type = request.Type, Command = command }));
            }

            return response;
        }

        [HttpPost]
        [ObservingNotAllowed]
        public JsonCommandResponse Execute(JsonCommandRequest request)
        {
            var response = new JsonCommandResponse();

            if (request != null && !string.IsNullOrEmpty(request.Type) && !string.IsNullOrEmpty(request.Command))
            {
                try
                {
                    ICommand concreteCommand = this.commandDeserializer.Deserialize(request.Type, request.Command);
                    ICommand transformedCommand = this.commandTransformator.TransformCommandIfNeeded(concreteCommand);

                    switch (transformedCommand)
                    {
                        case SetFlagToAnswerCommand setFlagCommand:
                            this.interviewFactory.SetFlagToQuestion(setFlagCommand.InterviewId,
                                Identity.Create(setFlagCommand.QuestionId, setFlagCommand.RosterVector), true);
                            break;
                        case RemoveFlagFromAnswerCommand removeFlagCommand:
                            this.interviewFactory.SetFlagToQuestion(removeFlagCommand.InterviewId,
                                Identity.Create(removeFlagCommand.QuestionId, removeFlagCommand.RosterVector), false);
                            break;
                        case HardDeleteInterview deleteInterview:
                            this.commandService.Execute(transformedCommand);
                            this.interviewFactory.RemoveInterview(deleteInterview.InterviewId);
                            break;
                        case RejectInterviewCommand rejectInterview:
                            this.commandService.Execute(transformedCommand);
                            CompleteCalendarEventIfExists(rejectInterview.InterviewId, rejectInterview.UserId);
                            break;
                        case ApproveInterviewCommand approveInterview:
                            this.commandService.Execute(transformedCommand);
                            CompleteCalendarEventIfExists(approveInterview.InterviewId, approveInterview.UserId);
                            break;
                        case HqRejectInterviewCommand rejectInterview:
                            this.commandService.Execute(transformedCommand);
                            CompleteCalendarEventIfExists(rejectInterview.InterviewId, rejectInterview.UserId);
                            break;
                        case HqApproveInterviewCommand approveInterview:
                            this.commandService.Execute(transformedCommand);
                            CompleteCalendarEventIfExists(approveInterview.InterviewId, approveInterview.UserId);
                            break;
                        case ChangeInterviewModeCommand changeInterviewMode:
                            this.commandService.Execute(changeInterviewMode);
                            break;
                        default:
                            this.commandService.Execute(transformedCommand);
                            break;
                    }

                    response.IsSuccess = true;
                }
                catch (Exception e)
                {
                    response.IsSuccess = false;

                    var domainEx = e.GetSelfOrInnerAs<InterviewException>();
                    if (domainEx == null)
                    {
                        throw new Exception(Strings.UnexpectedErrorOccurred, e);
                    }
                    else
                    {
                        response.DomainException = domainEx.Message;
                    }
                }
            }
            return response;
        }

        private void CompleteCalendarEventIfExists(Guid interviewId, Guid userId)
        {
            var calendarEvent = calendarEventService.GetActiveCalendarEventForInterviewId(interviewId);

            if (calendarEvent != null && !calendarEvent.IsCompleted())
            {
                var assignment = this.assignmentsStorage.GetAssignment(calendarEvent.AssignmentId);
                this.commandService.Execute(
                    new CompleteCalendarEventCommand(calendarEvent.PublicKey, userId, assignment.QuestionnaireId));
            }
        }
        
        public class JsonCommandBaseRequest
        {
            public string Type { get; set; }
        }

        public class JsonCommandRequest : JsonCommandBaseRequest
        {
            public string Command { get; set; }
        }

        public class JsonBundleCommandRequest : JsonCommandBaseRequest
        {
            public string[] Commands { get; set; }
        }
    }
}
