using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Hubs;

namespace WB.Enumerator.Native.WebInterview.Pipeline
{
    public class HubLifetimePipelineModule : HubPipelineModule
    {
        public override Func<IHubIncomingInvokerContext, Task<object>> BuildIncoming(
            Func<IHubIncomingInvokerContext, Task<object>> invoke)
        {
            return async context =>
            {
                // This is responsible for invoking every server-side Hub method in your SignalR app.
                return await InScopeExecutor.Current.ExecuteActionInScopeAsync(async sl =>
                {
                    if (context.Hub is WebInterview web)
                    {
                        web.SetServiceLocator(sl);
                    }

                    return await invoke(context);
                });
            };
        }
    }
}
