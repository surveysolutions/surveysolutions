using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Practices.ServiceLocation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;

namespace WB.UI.Headquarters.API.WebInterview
{
    public class PlainSignalRTransactionManager : HubPipelineModule
    {
        private IPlainTransactionManager transactionManager
            => ServiceLocator.Current.GetInstance<IPlainTransactionManagerProvider>().GetPlainTransactionManager();

        protected override bool OnBeforeIncoming(IHubIncomingInvokerContext context)
        {
            this.transactionManager.BeginTransaction();
            return base.OnBeforeIncoming(context);
        }

        protected override object OnAfterIncoming(object result, IHubIncomingInvokerContext context)
        {
            this.transactionManager.CommitTransaction();
            return base.OnAfterIncoming(result, context);
        }

        protected override void OnIncomingError(ExceptionContext exceptionContext, IHubIncomingInvokerContext invokerContext)
        {
            this.transactionManager.RollbackTransaction();
            base.OnIncomingError(exceptionContext, invokerContext); 
        }
    }
}