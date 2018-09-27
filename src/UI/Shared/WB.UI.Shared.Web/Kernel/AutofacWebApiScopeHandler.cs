using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.Infrastructure.Modularity.Autofac;

namespace WB.UI.Shared.Web.Kernel
{
    public class AutofacWebApiScopeHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var scope = ScopeManager.BeginScope();
            try
            {
                return base.SendAsync(request, cancellationToken);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
            finally
            {
                scope.Dispose();
            }
        }
    }
}
