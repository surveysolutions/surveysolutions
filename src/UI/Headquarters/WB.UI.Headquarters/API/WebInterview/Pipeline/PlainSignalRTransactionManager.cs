﻿using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Practices.ServiceLocation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;

namespace WB.UI.Headquarters.API.WebInterview.Pipeline
{
    public class PlainSignalRTransactionManager : HubPipelineModule
    {
        private IPlainTransactionManager transactionManager
            => ServiceLocator.Current.GetInstance<IPlainTransactionManagerProvider>().GetPlainTransactionManager();

        private ITransactionManager readTransactionManager
            => ServiceLocator.Current.GetInstance<ITransactionManagerProvider>().GetTransactionManager();

        protected override bool OnBeforeIncoming(IHubIncomingInvokerContext context)
        {
            this.transactionManager.BeginTransaction();
            this.readTransactionManager.BeginQueryTransaction();
            return base.OnBeforeIncoming(context);
        }

        protected override object OnAfterIncoming(object result, IHubIncomingInvokerContext context)
        {
            this.transactionManager.CommitTransaction();
            this.readTransactionManager.RollbackQueryTransaction();
            return base.OnAfterIncoming(result, context);
        }

        protected override void OnIncomingError(ExceptionContext exceptionContext, IHubIncomingInvokerContext invokerContext)
        {
            this.transactionManager.RollbackTransaction();
            this.readTransactionManager.RollbackQueryTransaction();
            base.OnIncomingError(exceptionContext, invokerContext);
        }
    }
}