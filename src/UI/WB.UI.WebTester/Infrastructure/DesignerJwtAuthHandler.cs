using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Infrastructure
{
    public class DesignerJwtAuthHandler : DelegatingHandler
    {
        private readonly IWebTesterJwtStore jwtStore;

        // Matches the standard 8-4-4-4-12 GUID format in /api/webtester/{questionnaireId}/...
        private static readonly Regex QuestionnaireIdPattern =
            new Regex(@"/api/webtester/([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public DesignerJwtAuthHandler(IWebTesterJwtStore jwtStore)
        {
            this.jwtStore = jwtStore;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri != null)
            {
                var match = QuestionnaireIdPattern.Match(request.RequestUri.PathAndQuery);
                if (match.Success && Guid.TryParse(match.Groups[1].Value, out var questionnaireId))
                {
                    var jwt = jwtStore.GetToken(questionnaireId);
                    if (jwt != null)
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
                    }
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
