using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR.Hubs;
using StackExchange.Exceptional;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Enumerator.Native.WebInterview.Pipeline
{
    public class SignalrErrorHandler : HubPipelineModule
    {
        private readonly ILogger logger;

        public SignalrErrorHandler(ILogger logger)
        {
            this.logger = logger;
        }

        [Localizable(false)]
        protected override void OnIncomingError(ExceptionContext exceptionContext, IHubIncomingInvokerContext invokerContext)
        {
            var hubName = invokerContext.Hub.GetType().Name;
            var actionName = invokerContext.MethodDescriptor.Name;
            var actionParameters = invokerContext.MethodDescriptor.Parameters
                .Select((p, index) => $"{p.Name}={invokerContext.Args[index]}")
                .ToList();

            var data = new Dictionary<string, string>
            {
                ["HubName"] = hubName,
                ["actionName"] = actionName,
                ["actionParams"] = string.Join(@", ", actionParameters),
                ["connectionId"] = invokerContext.Hub.Context.ConnectionId
            };

            (invokerContext.Hub as IErrorDetailsProvider)?.FillExceptionData(data);
            
            var message = $"SignalR: {exceptionContext.Error.Message}. {Delimiter}{string.Join(Delimiter, data.Select(kv => $"{kv.Key}: {kv.Value}"))}{Delimiter}";

            this.logger.Error(message, exceptionContext.Error);

            if (!(exceptionContext.Error is InterviewAccessException))
                new Exception(message, exceptionContext.Error)
                    .Log(HttpContext.Current, customData: data);

            base.OnIncomingError(exceptionContext, invokerContext);
        }
        const string Delimiter = "\r\n";
    }
}
