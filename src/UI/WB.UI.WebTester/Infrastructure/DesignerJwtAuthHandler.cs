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
            var questionnaireId = ExtractQuestionnaireId(request.RequestUri);
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

        private static Guid? ExtractQuestionnaireId(Uri? uri)
        {
            if (uri == null) return null;
            foreach (var segment in uri.Segments)
            {
                if (Guid.TryParse(segment.Trim('/'), out var guid))
                    return guid;
            }
            return null;
        }
    }
}
