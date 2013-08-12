﻿using System.Collections.Generic;
using Ncqrs.Commanding;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.UI.Shared.Web;
using WB.UI.Shared.Web.CommandDeserialization;
using System;
using System.Web.Mvc;
using Main.Core.Domain;
using Ncqrs.Commanding.ServiceModel;
using Web.Supervisor.Code;
using Web.Supervisor.Code.CommandTransformation;

namespace Web.Supervisor.Controllers
{
    public class CommandController : Controller
    {
        private readonly ICommandService commandService;
        private readonly ICommandDeserializer commandDeserializer;
        private readonly ILogger logger;

        public CommandController(ICommandService commandService, ICommandDeserializer commandDeserializer, ILogger logger)
        {
            this.commandService = commandService;
            this.commandDeserializer = commandDeserializer;
            this.logger = logger;
        }

        [HttpPost]
        public JsonResult ExecuteCommands(string type, string[] commands)
        {
            var errors = new List<string>();
            foreach (var command in commands)
            {
                try
                {
                    var concreteCommand = this.commandDeserializer.Deserialize(type, command);
                    this.commandService.Execute(concreteCommand);
                }
                catch (Exception e)
                {
                    var domainEx = e.As<DomainException>();
                    if (domainEx == null)
                    {
                        this.logger.Error("Unexpected error occurred", e);
                        errors.Add(string.Format("Unexpected error occurred. Please contact support via following email: <a href=\"mailto:{0}\">{0}</a>", AppSettings.Instance.AdminEmail));
                    }
                    else
                    {
                        errors.Add(domainEx.Message);
                    }
                }
            }
            return this.Json(errors.Count == 0 ? (object)new {status = "ok"} : new { status ="error", errors = errors });
        }

        [HttpPost]
        public JsonResult Execute(string type, string command)
        {
            string error = string.Empty;
            try
            {
                var concreteCommand = this.commandDeserializer.Deserialize(type, command);
                var transformedCommand = new CommandTransformator().TransformCommnadIfNeeded(type, concreteCommand);
                this.commandService.Execute(transformedCommand);
            }
            catch (Exception e)
            {
                var domainEx = e.As<DomainException>();
                if (domainEx == null)
                {
                    this.logger.Error("Unexpected error occurred", e);
                    error =
                        string.Format(
                            "Unexpected error occurred. Please contact support via following email: <a href=\"mailto:{0}\">{0}</a>",
                            AppSettings.Instance.AdminEmail);
                }
                else
                {
                    error = domainEx.Message;
                }
            }

            return this.Json(string.IsNullOrEmpty(error) ? (object)new { } : new { error = error });
        }

    }
}