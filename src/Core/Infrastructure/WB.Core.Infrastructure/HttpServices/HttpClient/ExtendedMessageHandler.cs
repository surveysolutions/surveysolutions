﻿using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.Portable.Implementation
{
    public class ExtendedMessageHandler : DelegatingHandler
    {
        private readonly IHttpStatistician statistician;

        public ExtendedMessageHandler(HttpMessageHandler innerHandler, IHttpStatistician statistician) 
            : base(innerHandler)
        {
            this.statistician = statistician;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var call = new HttpCall(request);

            call.DurationStopwatch.Start();
            try
            {
                call.Response = await InnerSendAsync(call, request, cancellationToken, times: 3)
                    .ConfigureAwait(false);
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
                call.DurationStopwatch.Stop();
                statistician?.CollectHttpCallStatistics(call);
            }
        }

        private async Task<HttpResponseMessage> InnerSendAsync(HttpCall call, 
            HttpRequestMessage request, CancellationToken cancellationToken, int times)
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
