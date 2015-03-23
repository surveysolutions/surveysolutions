using System;
using System.Web.Mvc;
using Ncqrs.Commanding;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.SurveyManagement.Web.Code.CommandTransformation;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Shared.Web.CommandDeserialization;
using WB.UI.Shared.Web.Filters;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
    [ApiValidationAntiForgeryToken]
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
                    ICommand transformedCommand = new CommandTransformator().TransformCommnadIfNeeded(concreteCommand);
                    this.CommandService.Execute(transformedCommand);

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