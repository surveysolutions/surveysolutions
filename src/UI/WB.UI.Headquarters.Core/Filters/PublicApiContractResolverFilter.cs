﻿using System;
using System.Buffers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace WB.UI.Headquarters.Filters
{
    public class PublicApiContractResolverFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext ctx)
        {
            if (ctx.Controller?.GetType().Namespace?.EndsWith("PublicApi", StringComparison.Ordinal) == false)
            {
                return;
            }

            var newtonsoftJsonOutputFormatter = new NewtonsoftJsonOutputFormatter(
                new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver()
                },
                ArrayPool<char>.Shared, new MvcOptions());
            if (ctx.Result is ObjectResult objectResult)
            {
                objectResult.Formatters.Add(newtonsoftJsonOutputFormatter);
            }
            else if (ctx.Result is OkObjectResult okObjectResult)
            {
                okObjectResult.Formatters.Add(newtonsoftJsonOutputFormatter);
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
        }
    }
}
