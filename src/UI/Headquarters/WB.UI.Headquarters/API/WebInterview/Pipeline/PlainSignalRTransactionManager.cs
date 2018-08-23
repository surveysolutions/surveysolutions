using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Hubs;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Infrastructure.Native.Ioc;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.API.WebInterview.Pipeline
{
    public class PlainSignalRTransactionManager : HubPipelineModule
    {
        public override Func<IHubIncomingInvokerContext, Task<object>> BuildIncoming(
            Func<IHubIncomingInvokerContext, Task<object>> invoke)
        {
            return base.BuildIncoming(context =>
            {
                using (new NinjectAmbientScope())
                {
                    var unitOfWork = ServiceLocator.Current.GetInstance<IUnitOfWork>();
                    
                    try
                    {
                        var result = (invoke(context)).Result;
                        unitOfWork.AcceptChanges();
                        return Task.FromResult((object)result);
                    }
                    catch (Exception)
                    {
                        unitOfWork.Dispose();
                        throw;
                    }
                }
            });
        }

        protected override bool OnBeforeIncoming(IHubIncomingInvokerContext context)
        {
            // TODO CHECK HOW TO HANDLE TRANSACTIONS HERE
            return base.OnBeforeIncoming(context);
        }

        protected override object OnAfterIncoming(object result, IHubIncomingInvokerContext context)
        {
            return base.OnAfterIncoming(result, context);
        }

        protected override void OnIncomingError(ExceptionContext exceptionContext, IHubIncomingInvokerContext invokerContext)
        {
            base.OnIncomingError(exceptionContext, invokerContext);
        }
    }
}
