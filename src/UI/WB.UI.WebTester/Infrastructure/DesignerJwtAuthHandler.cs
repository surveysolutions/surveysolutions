using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Infrastructure
{
    /// <summary>
    /// <c>DelegatingHandler</c> on the <c>IDesignerWebTesterApi</c> Refit HTTP client.
    /// Attaches the delegated JWT (<c>Authorization: Bearer …</c>) and per-request tracing
    /// headers (<c>X-Correlation-Id</c>, <c>X-User-Id</c>) to every outbound Designer API call.
    /// </summary>
    /// <remarks>
    /// <para><b>How the interview ID is resolved</b></para>
    /// <para>
    /// The JWT and user context are keyed by <c>interviewId</c> (unique per test run) stored in
    /// <see cref="DesignerJwtContext.InterviewId"/> — an <see cref="System.Threading.AsyncLocal{T}"/>
    /// that flows naturally into child <see cref="System.Threading.Tasks.Task"/>s and async
    /// continuations. The handler does NOT inspect the outbound URL to extract a questionnaire or
    /// interview ID; the caller is responsible for setting the context before any Designer API
    /// call is made.
    /// </para>
    /// <para><b>Where the context must be set</b></para>
    /// <list type="number">
    ///   <item>
    ///     <term><c>WebTesterController.Run</c> (code-exchange path)</term>
    ///     <description>
    ///       Set immediately after a successful code exchange, <em>before</em> calling
    ///       <c>StartImportQuestionnaireAndCreateInterview</c>. The <c>AsyncLocal</c> value is
    ///       captured by the background import task and remains available throughout the entire
    ///       async import chain without any further action.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term><c>WebTesterSessionAuthorizeAttribute</c> (subsequent HTTP requests)</term>
    ///     <description>
    ///       Set after the session and JWT store checks pass, making the interview ID available
    ///       for any Designer API calls triggered during that request (e.g. scenario proxy).
    ///     </description>
    ///   </item>
    /// </list>
    /// <para><b>Consequence when the context is absent</b></para>
    /// <para>
    /// If <see cref="DesignerJwtContext.InterviewId"/> is <c>null</c> the handler forwards the
    /// request without an <c>Authorization</c> header. Designer's protected endpoints will return
    /// <c>401 Unauthorized</c> and the import task will fault (status → <c>Error</c>).
    /// This is an explicit fail-fast contract: callers must set the context; the handler never
    /// guesses or falls back to URL parsing.
    /// </para>
    /// </remarks>
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
