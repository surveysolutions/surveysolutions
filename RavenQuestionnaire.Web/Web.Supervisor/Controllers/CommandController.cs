using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Main.Core.Domain;
using Ncqrs.Commanding;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.UI.Shared.Web;
using WB.UI.Shared.Web.CommandDeserialization;
using Web.Supervisor.Code;
using Web.Supervisor.Code.CommandTransformation;

namespace Web.Supervisor.Controllers
{
    public class CommandController : Controller
    {
        private readonly ICommandDeserializer commandDeserializer;
        private readonly ICommandService commandService;
        private readonly IGlobalInfoProvider globalInfo;
        private readonly ILogger logger;

        public CommandController(ICommandService commandService, ICommandDeserializer commandDeserializer, ILogger logger,
                                 IGlobalInfoProvider globalInfo)
        {
            this.commandService = commandService;
            this.commandDeserializer = commandDeserializer;
            this.logger = logger;
            this.globalInfo = globalInfo;
        }

        [HttpPost]
        public JsonResult ExecuteCommands(string type, string[] commands)
        {
            var errors = new List<string>();
            var failedCommands = new List<ICommand>();
            foreach (string command in commands)
            {
                ICommand concreteCommand;
                ICommand transformedCommand;
                try
                {
                    concreteCommand = this.commandDeserializer.Deserialize(type, command);
                    transformedCommand = new CommandTransformator().TransformCommnadIfNeeded(type, concreteCommand,
                                                                                             this.globalInfo);
                }
                catch (CommandDeserializationException e)
                {
                    errors.Add(e.Message);
                    continue;
                }

                try
                {
                    this.commandService.Execute(transformedCommand);
                }
                catch (Exception e)
                {
                    string error;
                    if (IsExceptionRaisedByAggregateRoot(e, out error))
                    {
                        errors.Add(error);
                    }
                    else
                    {
                        this.logger.Error("Unexpected error occurred", e);
                        errors.Add(
                            string.Format(
                                "Unexpected error occurred. Please contact support via following email: <a href=\"mailto:{0}\">{0}</a>",
                                AppSettings.Instance.AdminEmail));
                    }

                    failedCommands.Add(concreteCommand);
                }
            }
            return
                this.Json(errors.Count == 0
                              ? (object) new {status = "ok"}
                              : new {status = "error", errors, failedCommands});
        }

        [HttpPost]
        public JsonResult Execute(string type, string command)
        {
            string error = string.Empty;
            try
            {
                ICommand concreteCommand = this.commandDeserializer.Deserialize(type, command);
                ICommand transformedCommand = new CommandTransformator().TransformCommnadIfNeeded(type, concreteCommand,
                                                                                                  this.globalInfo);
                this.commandService.Execute(transformedCommand);
            }
            catch (Exception e)
            {
                if (!IsExceptionRaisedByAggregateRoot(e, out error))
                {
                    this.logger.Error("Unexpected error occurred", e);
                    error =
                        string.Format(
                            "Unexpected error occurred. Please contact support via following email: <a href=\"mailto:{0}\">{0}</a>",
                            AppSettings.Instance.AdminEmail);
                }
            }

            return this.Json(string.IsNullOrEmpty(error) ? (object) new {} : new {error});
        }

        private bool IsExceptionRaisedByAggregateRoot(Exception exception, out string domainMessage)
        {
            var interviewException = exception.As<InterviewException>();
            if (interviewException == null)
            {
                var domainException = exception.As<DomainException>();
                if (domainException == null)
                {
                    domainMessage = string.Empty;
                    return false;
                }
                else
                {
                    domainMessage = domainException.Message;
                    return true;
                }
            }
            else
            {
                domainMessage = interviewException.Message;
                return true;
            }
        }
    }
}