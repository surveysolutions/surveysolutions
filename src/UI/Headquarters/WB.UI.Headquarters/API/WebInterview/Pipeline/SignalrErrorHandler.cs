using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Elmah;
using Microsoft.AspNet.SignalR.Hubs;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.WebInterview.Pipeline
{
    public class SignalrErrorHandler : HubPipelineModule
    {
        private ILogger logger => ServiceLocator.Current.GetInstance<ILogger>();

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
                ["actionParams"] = string.Join(", ", actionParameters),
                ["connectionId"] = invokerContext.Hub.Context.ConnectionId
            };

            (invokerContext.Hub as IErrorDetailsProvider)?.FillExceptionData(data);
            
            var message = $"SignalR: {exceptionContext.Error.Message}. {delimiter}{string.Join(delimiter, data.Select(kv => $"{kv.Key}: {kv.Value}"))}{delimiter}";

            this.logger.Error(message, exceptionContext.Error);

            if (!(exceptionContext.Error is WebInterviewAccessException))
                ErrorLog.GetDefault(HttpContext.Current).Log(new Error(new Exception(message, exceptionContext.Error)));

            base.OnIncomingError(exceptionContext, invokerContext);
        }
        const string delimiter = "\r\n";
    }
}