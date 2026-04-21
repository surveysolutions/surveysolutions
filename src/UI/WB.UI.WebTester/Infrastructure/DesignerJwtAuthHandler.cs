using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Infrastructure
{
    /// <summary>
    /// Attaches the delegated JWT and tracing headers to every outbound Designer API request.
    /// The interview ID is resolved from <see cref="DesignerJwtContext.InterviewId"/>, which is
    /// an <see cref="System.Threading.AsyncLocal{T}"/> set in two places:
    /// <list type="bullet">
    ///   <item><c>WebTesterController.Run</c> — before starting the background import task;
    ///         the value flows through the async call chain into child Tasks.</item>
    ///   <item><c>WebTesterSessionAuthorizeAttribute</c> — for subsequent HTTP requests
    ///         (Interview, Loading, ScenariosProxy, …). </item>
    /// </list>
    /// </summary>
    public class DesignerJwtAuthHandler : DelegatingHandler
    {
        private readonly IWebTesterJwtStore jwtStore;
        private readonly IUserContextStore userContextStore;

        public DesignerJwtAuthHandler(IWebTesterJwtStore jwtStore, IUserContextStore userContextStore)
        {
            this.jwtStore = jwtStore;
            this.userContextStore = userContextStore;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var interviewId = DesignerJwtContext.InterviewId;
            if (interviewId.HasValue)
            {
                var jwt = jwtStore.GetToken(interviewId.Value);
                if (jwt != null)
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

                var ctx = userContextStore.Get(interviewId.Value);
                if (!string.IsNullOrEmpty(ctx?.CorrelationId))
                    request.Headers.TryAddWithoutValidation("X-Correlation-Id", ctx.CorrelationId);
                if (!string.IsNullOrEmpty(ctx?.UserId))
                    request.Headers.TryAddWithoutValidation("X-User-Id", ctx.UserId);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
