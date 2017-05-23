﻿using System.Text;
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Shared.Web.Controllers;

namespace WB.UI.Headquarters.Controllers
{
    public abstract class BaseController : BaseMessageDisplayController
    {
        protected readonly ICommandService CommandService;

        protected readonly ILogger Logger;

        protected BaseController(ICommandService commandService, ILogger logger)
        {
            this.CommandService = commandService;
            this.Logger = logger;
        }

        protected ActionResult JsonCamelCase(object result)
        {
            var response = JsonConvert.SerializeObject(
                result,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
            return new ContentResult
            {
                ContentType = "application/json",
                Content = response,
                ContentEncoding = Encoding.UTF8
            };
        }
    }
}