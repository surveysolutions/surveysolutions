using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Infrastructure
{
    public class DesignerJwtAuthHandler : DelegatingHandler
    {
        private readonly IWebTesterJwtStore jwtStore;

        public DesignerJwtAuthHandler(IWebTesterJwtStore jwtStore)
        {
            this.jwtStore = jwtStore;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var questionnaireId = WebTesterApiContext.Current;
            if (questionnaireId.HasValue)
            {
                var jwt = jwtStore.GetToken(questionnaireId.Value);
                if (jwt != null)
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
