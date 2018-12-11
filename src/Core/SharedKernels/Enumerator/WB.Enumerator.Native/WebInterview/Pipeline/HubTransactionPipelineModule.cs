using Microsoft.AspNet.SignalR.Hubs;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Enumerator.Native.WebInterview.Pipeline
{
    public class HubTransactionPipelineModule : HubPipelineModule
    {
        IServiceLocator GetScope(IHubIncomingInvokerContext context)
        {
            return context.Hub is WebInterview webHub ? webHub.ServiceLocator : null;
        }

        protected override object OnAfterIncoming(object result, IHubIncomingInvokerContext context)
        {
            var uow = GetScope(context)?.GetInstance<IUnitOfWork>();
            uow?.AcceptChanges();
            uow?.Dispose();
            return base.OnAfterIncoming(result, context);
        }

        protected override void OnIncomingError(ExceptionContext exceptionContext, IHubIncomingInvokerContext context)
        {
            var uow = GetScope(context)?.GetInstance<IUnitOfWork>();
            uow?.Dispose();
            base.OnIncomingError(exceptionContext, context);
        }
    }
}