using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Http;
using Ncqrs.Commanding;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Headquarters.Authentication;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.UI.Headquarters.Api.Models;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.UI.Headquarters.Api
{
    [Authorize(Roles = ApplicationRoles.Headquarter)]
    public class CommandApiController : ApiController
    {
        private readonly ICommandDeserializer commandDeserializer;
        private readonly ICommandService commandService;
        private readonly ILogger logger;

        public CommandApiController(ICommandDeserializer commandDeserializer,
            ICommandService commandService,
            ILogger logger)
        {
            if (commandDeserializer == null) throw new ArgumentNullException("commandDeserializer");
            if (commandService == null) throw new ArgumentNullException("commandService");
            if (logger == null) throw new ArgumentNullException("logger");
            this.commandDeserializer = commandDeserializer;
            this.commandService = commandService;
            this.logger = logger;
        }

        [HttpPost]
        public JsonBundleCommandResponse ExecuteCommands(JsonBundleCommandRequest request)
        {
            var response = new JsonBundleCommandResponse();

            if (request == null || string.IsNullOrEmpty(request.Type) || request.Commands == null)
                throw new NullReferenceException();

            foreach (var command in request.Commands)
            {
                response.CommandStatuses.Add(
                    this.Execute(new JsonCommandRequest() {Type = request.Type, Command = command}));
            }

            return response;
        }

        [HttpPost]
        public JsonCommandResponse Execute(JsonCommandRequest request)
        {
            var response = new JsonCommandResponse();
            if (request != null && !string.IsNullOrEmpty(request.Type) && !string.IsNullOrEmpty(request.Command))
            {
                try
                {
                    ICommand concreteCommand = this.commandDeserializer.Deserialize(request.Type, request.Command);
                    response.CommandId = concreteCommand.CommandIdentifier;
                    ICommand transformedCommand = new CommandTransformator(new HttpContextWrapper(HttpContext.Current)).TransformCommnadIfNeeded(request.Type,
                        concreteCommand);
                    this.commandService.Execute(transformedCommand);

                    response.IsSuccess = true;
                }
                catch (OverflowException e)
                {
                    response.IsSuccess = true;
                    response.DomainException = e.Message;
                }
                catch (Exception e)
                {
                    var domainEx = e.GetSelfOrInnerAs<InterviewException>();
                    if (domainEx == null)
                    {
                        this.logger.Error("Unexpected error occurred", e);
                    }
                    else
                    {
                        response.IsSuccess = true;
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

        public class JsonCommandResponse
        {
            public Guid CommandId { get; set; }
            public string DomainException { get; set; }
            public bool IsSuccess = false;
        }

        public class JsonBundleCommandRequest : JsonCommandBaseRequest
        {
            public string[] Commands { get; set; }
        }

        public class JsonBundleCommandResponse
        {
            public List<JsonCommandResponse> CommandStatuses { get; set; }
            public JsonBundleCommandResponse()
            {
                this.CommandStatuses = new List<JsonCommandResponse>();
            }
        }
    }
}