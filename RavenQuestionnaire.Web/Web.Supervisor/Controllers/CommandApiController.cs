using System;
using System.Web.Mvc;
using Main.Core.Domain;
using Ncqrs.Commanding;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging;
using WB.UI.Shared.Web;
using WB.UI.Shared.Web.CommandDeserialization;
using Web.Supervisor.Code.CommandTransformation;
using Web.Supervisor.Models;

namespace Web.Supervisor.Controllers
{
    [Authorize(Roles = "Headquarter, Supervisor")]
    public class CommandApiController : BaseApiController
    {
        private readonly ICommandDeserializer commandDeserializer;

        public CommandApiController(
            ICommandService commandService, ICommandDeserializer commandDeserializer, ILogger logger,
                                 IGlobalInfoProvider globalInfo)
            : base(commandService, globalInfo, logger)
        {
            this.commandDeserializer = commandDeserializer;
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
                    ICommand transformedCommand = new CommandTransformator().TransformCommnadIfNeeded(request.Type,
                                                                                                      concreteCommand,
                                                                                                      this.GlobalInfo);
                    this.CommandService.Execute(transformedCommand);

                    response.IsSuccess = true;
                }
                catch (Exception e)
                {
                    var domainEx = e.As<DomainException>();
                    if (domainEx == null)
                    {
                        this.Logger.Error("Unexpected error occurred", e);
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

        public class JsonBundleCommandRequest : JsonCommandBaseRequest
        {
            public string[] Commands { get; set; }
        }
    }
}