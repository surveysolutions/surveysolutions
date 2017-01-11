using System;
using System.Linq;
using Elmah;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.UI.Headquarters.API.WebInterview
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

            if (invokerContext.Hub is WebInterview)
                actionParameters.Insert(0, $"interviewId={invokerContext.Hub.Clients.Caller.interviewId}");

            var errorMessage = $"SignalR: {hubName}.{actionName}({string.Join(@", ", actionParameters)})";

            this.logger.Error(errorMessage, exceptionContext.Error);
            ErrorSignal.FromCurrentContext().Raise(new Exception(errorMessage, exceptionContext.Error));

            base.OnIncomingError(exceptionContext, invokerContext);
        }
    }
}