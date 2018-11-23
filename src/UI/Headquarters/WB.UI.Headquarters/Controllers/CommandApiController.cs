using System;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Code.CommandTransformation;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models;
using WB.UI.Shared.Web.CommandDeserialization;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [AuthorizeOr403(Roles = "Administrator, Headquarter, Supervisor")]
    [ApiValidationAntiForgeryToken]
    public class CommandApiController : BaseApiController
    {
        private readonly ICommandDeserializer commandDeserializer;
        private readonly IInterviewFactory _interviewFactory;
        private ICommandTransformator commandTransformator;
        private const string DefaultErrorMessage = "Unexpected error occurred";

        public CommandApiController(
            ICommandService commandService, ICommandDeserializer commandDeserializer, ILogger logger,
            IInterviewFactory interviewFactory, ICommandTransformator commandTransformator)
            : base(commandService, logger)
        {
            this.commandDeserializer = commandDeserializer;
            _interviewFactory = interviewFactory;
            this.commandTransformator = commandTransformator;
        }

        [HttpPost]
        [ObserverNotAllowedApi]
        public JsonBundleCommandResponse ExecuteCommands(JsonBundleCommandRequest request)
        {
            var response = new JsonBundleCommandResponse();

            if (request == null || string.IsNullOrEmpty(request.Type) || request.Commands == null)
                throw new NullReferenceException();

            foreach (var command in request.Commands)
            {
                response.CommandStatuses.Add(this.Execute(new JsonCommandRequest() {Type = request.Type, Command = command}));
            }

            return response;
        }

        [HttpPost]
        [ObserverNotAllowedApi]
        public JsonCommandResponse Execute(JsonCommandRequest request)
        {
            var response = new JsonCommandResponse();
            
            if (request != null && !string.IsNullOrEmpty(request.Type) && !string.IsNullOrEmpty(request.Command))
            {
                try
                {
                    ICommand concreteCommand = this.commandDeserializer.Deserialize(request.Type, request.Command);
                    ICommand transformedCommand = this.commandTransformator.TransformCommnadIfNeeded(concreteCommand);

                    switch (transformedCommand)
                    {
                        case SetFlagToAnswerCommand setFlagCommand:
                            this._interviewFactory.SetFlagToQuestion(setFlagCommand.InterviewId,
                                Identity.Create(setFlagCommand.QuestionId, setFlagCommand.RosterVector), true);
                            break;
                        case RemoveFlagFromAnswerCommand removeFlagCommand:
                            this._interviewFactory.SetFlagToQuestion(removeFlagCommand.InterviewId,
                                Identity.Create(removeFlagCommand.QuestionId, removeFlagCommand.RosterVector), false);
                            break;
                        case HardDeleteInterview deleteInterview:
                            this.CommandService.Execute(transformedCommand);
                            this._interviewFactory.RemoveInterview(deleteInterview.InterviewId);
                            break;
                        default:
                            this.CommandService.Execute(transformedCommand);
                            break;
                    }

                    response.IsSuccess = true;
                }
                catch (OverflowException e)
                {
                    this.Logger.Error(DefaultErrorMessage, e);
                    response.IsSuccess = false;
                    response.DomainException = Strings.UnexpectedErrorOccurred;
                }
                catch (Exception e)
                {
                    response.IsSuccess = false;

                    var domainEx = e.GetSelfOrInnerAs<InterviewException>();
                    if (domainEx == null)
                    {
                        this.Logger.Error(DefaultErrorMessage, e);
                        response.DomainException = Strings.UnexpectedErrorOccurred;
                    }
                    else
                    {
                        response.DomainException = domainEx.Message;
                    }
                }
            }
            return response;
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
