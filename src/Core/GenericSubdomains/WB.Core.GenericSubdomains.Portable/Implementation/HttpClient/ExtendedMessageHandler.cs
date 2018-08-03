using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.Portable.Implementation
{
    public class ExtendedMessageHandler : DelegatingHandler
    {
        private readonly IHttpStatistician statistician;

        public ExtendedMessageHandler(HttpMessageHandler innerHandler, IHttpStatistician statistician) : base(innerHandler)
        {
            this.statistician = statistician;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //var call = HttpCall.Get(request);
            var call = new HttpCall(request);

            call.StartedUtc = DateTime.UtcNow;
            try
            {
                call.Response = await InnerSendAsync(call, request, cancellationToken).ConfigureAwait(false);
                call.Response.RequestMessage = request;
                if (call.IsSucceeded)
                    return call.Response;

                throw new ExtendedMessageHandlerException(call, null);
            }
            catch (Exception ex)
            {
                call.Exception = ex;
                throw;
            }
            finally
            {
                call.EndedUtc = DateTime.UtcNow;
                statistician?.CollectHttpCallStatistics(call);
            }
        }

        private async Task<HttpResponseMessage> InnerSendAsync(HttpCall call, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new ExtendedMessageHandlerException(call, ex);
            }
        }
    }
}
