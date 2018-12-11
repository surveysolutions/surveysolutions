using System.Threading;
using Autofac;
using Microsoft.AspNet.SignalR.Hubs;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Enumerator.Native.WebInterview.Pipeline
{
    public class HubLifetimePipelineModule : HubPipelineModule
    {
        private readonly ILifetimeScope serviceLocator;
        static readonly AsyncLocal<ILifetimeScope> Scope = new AsyncLocal<ILifetimeScope>();

        public HubLifetimePipelineModule(ILifetimeScope serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        protected override bool OnBeforeIncoming(IHubIncomingInvokerContext context)
        {
            Scope.Value = serviceLocator.BeginLifetimeScope(AutofacServiceLocatorConstants.UnitOfWorkScope);
            if (context.Hub is WebInterview hub)
            {
                hub.SetServiceLocator(Scope.Value.Resolve<IServiceLocator>());
            }
            return base.OnBeforeIncoming(context);
        }

        protected override object OnAfterIncoming(object result, IHubIncomingInvokerContext context)
        {
            var uow = Scope.Value.Resolve<IUnitOfWork>();
            uow.AcceptChanges();
            uow.Dispose();
            Scope.Value.Dispose();
            return base.OnAfterIncoming(result, context);
        }

        protected override void OnIncomingError(ExceptionContext exceptionContext, IHubIncomingInvokerContext invokerContext)
        {
            var uow = Scope.Value.Resolve<IUnitOfWork>();
            uow.Dispose();
            Scope.Value.Dispose();
            base.OnIncomingError(exceptionContext, invokerContext);
        }
    }
}
