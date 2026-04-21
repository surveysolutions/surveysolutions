using Refit;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Api;
using WB.UI.WebTester.Infrastructure;
using WB.UI.WebTester.Infrastructure.AppDomainSpecific;
using WB.UI.WebTester.Resources;
using WB.UI.WebTester.Services;
using WB.UI.WebTester.Services.Implementation;

namespace WB.UI.WebTester.Controllers
{
    [Route("WebTester")]
    [WebTesterSessionAuthorize]
    public class WebTesterController : Controller
    {
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IEvictionNotifier evictionService;
        private readonly IImportQuestionnaireAndCreateInterviewService interviewFactory;
        private readonly IOptions<TesterConfiguration> testerConfig;
        private readonly IWebTesterJwtStore jwtStore;
        private readonly ICodeExchangeClient codeExchangeClient;
        private readonly IUserContextStore userContextStore;
        private readonly IWebTesterSessionService sessionService;
        private readonly ILogger<WebTesterController> logger;

        public WebTesterController(
            IStatefulInterviewRepository statefulInterviewRepository,
            IEvictionNotifier evictionService,
            IImportQuestionnaireAndCreateInterviewService interviewFactory,
            IOptions<TesterConfiguration> testerConfig,
            IWebTesterJwtStore jwtStore,
            ICodeExchangeClient codeExchangeClient,
            IUserContextStore userContextStore,
            IWebTesterSessionService sessionService,
            ILogger<WebTesterController> logger)
        {
            this.statefulInterviewRepository = statefulInterviewRepository ?? throw new ArgumentNullException(nameof(statefulInterviewRepository));
            this.evictionService = evictionService;
            this.interviewFactory = interviewFactory;
            this.testerConfig = testerConfig;
            this.jwtStore = jwtStore ?? throw new ArgumentNullException(nameof(jwtStore));
            this.codeExchangeClient = codeExchangeClient ?? throw new ArgumentNullException(nameof(codeExchangeClient));
            this.userContextStore = userContextStore ?? throw new ArgumentNullException(nameof(userContextStore));
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Entry point for a WebTester session. Implements the <b>one-time code exchange flow</b>:
        /// the browser arrives here with <c>?code=&lt;one-time-code&gt;</c> issued by Designer's
        /// <c>GET /api/questionnaire/WebTest/{id}</c>. The code is exchanged <b>server-to-server</b>
        /// for a short-lived delegated JWT that is stored in <see cref="IWebTesterJwtStore"/> and
        /// never returned to the browser.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A fresh <c>interviewId</c> (<see cref="Guid.NewGuid"/>) is generated on every run so
        /// that concurrent test sessions for the same questionnaire (different browser tabs or
        /// different users) each receive an isolated interview aggregate and JWT — they can no
        /// longer evict or overwrite each other.
        /// </para>
        /// <para>Subsequent browser requests (interview pages, scenario proxy) are authorised via the
        /// per-<c>interviewId</c> ASP.NET Session entry written by
        /// <see cref="IWebTesterSessionService.AuthorizeQuestionnaire"/> during the exchange.</para>
        /// <para>
        /// If <paramref name="code"/> is absent the request is still permitted <b>only</b> when the
        /// browser session already holds a valid session entry AND the JWT has not yet expired in the
        /// store (e.g., browser refresh). Otherwise the request is redirected to an error page.
        /// </para>
        /// <para>
        /// <b>Note:</b> there is no <c>?jwt=</c> query parameter and no <c>X-WebTester-Token</c>
        /// response header — those patterns belonged to an earlier design and are not used.
        /// </para>
        /// </remarks>
        [HttpGet]
        [Route("Run/{questionnaireId:Guid}")]
        [SkipWebTesterSessionAuthorize]
        public async Task<IActionResult> Run(Guid questionnaireId, Guid? sid, int? scenarioId = null,
            [FromQuery] string? code = null)
        {
            Guid interviewId;

            // Exchange one-time code for delegated JWT (backend-to-backend)
            if (!string.IsNullOrWhiteSpace(code))
            {
                // Sanity-check: a valid base64url-encoded 32-byte code is ~43 chars
                if (code.Length > 200)
                {
                    logger.LogWarning(
                        "Rejected suspiciously long code parameter. Length={Length}, " +
                        "QuestionnaireId={QuestionnaireId}",
                        code.Length, questionnaireId);
                    return this.RedirectToAction("QuestionnaireWithErrors", "Error");
                }

                var exchangeResult = await codeExchangeClient.ExchangeAsync(code);
                if (exchangeResult == null)
                {
                    // Exchange failed — no JWT available, Designer API calls will be rejected.
                    // Redirect to an error page rather than starting an import that will fail with 401.
                    logger.LogError(
                        "Code exchange returned no result — cannot start interview. " +
                        "QuestionnaireId={QuestionnaireId}, TraceId={TraceId}. " +
                        "Check that Designer is reachable and WebTester:ServiceApiKey matches.",
                        questionnaireId, HttpContext.TraceIdentifier);
                    return this.RedirectToAction("QuestionnaireWithErrors", "Error");
                }

                // Generate a unique interviewId for this run so concurrent sessions
                // for the same questionnaire don't collide.
                interviewId = Guid.NewGuid();

                // If this browser session previously had a run for this questionnaire,
                // evict the old interview now so memory is reclaimed immediately rather
                // than waiting for JWT TTL expiry.
                var previousInterviewId = sessionService.GetInterviewId(HttpContext.Session, questionnaireId);
                if (previousInterviewId.HasValue
                    && statefulInterviewRepository.Get(previousInterviewId.Value.FormatGuid()) != null)
                {
                    evictionService.Evict(previousInterviewId.Value);
                }

                jwtStore.StoreToken(interviewId, exchangeResult.AccessToken,
                    TimeSpan.FromSeconds(exchangeResult.ExpiresIn));
                userContextStore.Store(interviewId, new RequestUserContext
                {
                    UserId         = exchangeResult.UserId,
                    CorrelationId  = exchangeResult.CorrelationId,
                    DelegatedToken = exchangeResult.AccessToken
                });
                sessionService.AuthorizeQuestionnaire(HttpContext.Session, interviewId, questionnaireId);

                logger.LogInformation(
                    "Session started. UserId={UserId}, CorrelationId={CorrelationId}, " +
                    "QuestionnaireId={QuestionnaireId}, InterviewId={InterviewId}, " +
                    "TraceId={TraceId}, ServiceName=WB.WebTester",
                    exchangeResult.UserId ?? "anonymous",
                    exchangeResult.CorrelationId,
                    questionnaireId,
                    interviewId,
                    HttpContext.TraceIdentifier);
            }
            else
            {
                // No code provided — only allowed if this browser session already holds a valid
                // authorization AND the delegated JWT is still alive in the store (not expired).
                // If either is missing, starting the import would trigger 401s from Designer.
                var existingInterviewId = sessionService.GetInterviewId(HttpContext.Session, questionnaireId);
                bool sessionOk = existingInterviewId.HasValue
                    && sessionService.IsAuthorized(HttpContext.Session, existingInterviewId.Value);
                bool tokenOk = existingInterviewId.HasValue
                    && jwtStore.GetToken(existingInterviewId.Value) != null;

                if (!sessionOk || !tokenOk)
                {
                    logger.LogWarning(
                        "Run called without code and no active session/token. " +
                        "SessionOk={SessionOk}, TokenOk={TokenOk}, " +
                        "QuestionnaireId={QuestionnaireId}, TraceId={TraceId}",
                        sessionOk, tokenOk, questionnaireId, HttpContext.TraceIdentifier);
                    return this.RedirectToAction("QuestionnaireWithErrors", "Error");
                }

                // Reuse the existing interviewId for this refresh.
                interviewId = existingInterviewId!.Value;
            }

            // Make interviewId available to DesignerJwtAuthHandler for the background import Task.
            // AsyncLocal flows into child tasks, so the whole import chain will carry this value.
            DesignerJwtContext.InterviewId = interviewId;

            interviewFactory.StartImportQuestionnaireAndCreateInterview(
                questionnaireId, interviewId, sid, scenarioId);

            return this.View(new InterviewPageModel
            {
                Id = interviewId.ToString(),
            });
        }


        [Route("Status/{id:Guid}")]
        public IActionResult GetStatus(Guid id)
        {
            return Ok(interviewFactory.GetStatus(id)?.ToString());
        }
        
        [Route("Loading/{id:Guid}")]
        public IActionResult Loading(Guid id)
        {
            var status = interviewFactory.GetStatus(id);
            if (status == CreationResult.Loading)
            {
                return this.View("Loading", new InterviewPageModel
                {
                    Id = id.ToString(),
                });
            }

            interviewFactory.RemoveStatus(id);
            
            if (status is null or CreationResult.Error)
            {
                return this.RedirectToAction("QuestionnaireWithErrors", "Error");
            }
            else if (status == CreationResult.DataPartialRestored)
            {
                TempData["Message"] = Common.ReloadPartialInterviewErrorMessage;
            }
            else if (status == CreationResult.DataRestoreError)
            {
                TempData["Message"] = Common.ReloadInterviewErrorMessage;
            }
            
            var interview = statefulInterviewRepository.Get(id.FormatGuid()) as WebTesterStatefulInterview;
            if (interview?.Questionnaire.IsCoverPageSupported ?? false)
                return this.Redirect($"~/WebTester/Interview/{id.FormatGuid()}/Section/{interview?.Questionnaire.CoverPageSectionId.FormatGuid()}");
            else
                return this.Redirect($"~/WebTester/Interview/{id.FormatGuid()}/Cover");
        }

        [Route("Interview/{id}")]
        [Route("Interview/{id}/Cover")]
        [Route("Interview/{id}/Section/{url}")]
        [Route("Interview/{id}/Complete")]
        public IActionResult Interview(string id)
        {
            try
            {
                var interviewPageModel = GetInterviewPageModel(id);
                if (interviewPageModel == null)
                    return StatusCode(StatusCodes.Status404NotFound, string.Empty);

                return View(interviewPageModel);
            }
            catch (ApiException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                return StatusCode(StatusCodes.Status404NotFound, string.Empty);
            }
        }

        private InterviewPageModel? GetInterviewPageModel(string id)
        {
            var interview = statefulInterviewRepository.Get(id);

            if (interview == null)
            {
                return null;
            }

            WebTesterStatefulInterview webTesterInterview = (WebTesterStatefulInterview) interview;

            var questionnaire = (WebTesterPlainQuestionnaire)webTesterInterview.Questionnaire;

            var designerUrl = testerConfig.Value.DesignerAddress;
            var reloadQuestionnaireUrl =
                $"{designerUrl}/WebTesterReload/Index/{interview.QuestionnaireIdentity.QuestionnaireId}?interviewId={id}";

            var saveScenarioDesignerUrl = questionnaire.CanSaveScenario
                ? Url.Content($"~/api/ScenariosProxy/{interview.QuestionnaireIdentity.QuestionnaireId}")
                : null;
                
            var interviewPageModel = new InterviewPageModel
            {
                Id = id,
                CoverPageId = questionnaire.IsCoverPageSupported ? questionnaire.CoverPageSectionId.FormatGuid() : String.Empty,
                Title = $"{questionnaire.Title} | Web Tester",
                GoogleMapsKey = testerConfig.Value.GoogleMapApiKey,
                ReloadQuestionnaireUrl = reloadQuestionnaireUrl
            };
            
            interviewPageModel.GetScenarioUrl = Url.Content("~/api/ScenariosApi");
            interviewPageModel.SaveScenarioUrl = saveScenarioDesignerUrl;
            interviewPageModel.DesignerUrl = designerUrl;
            interviewPageModel.QuestionnaireId = interview.QuestionnaireIdentity.QuestionnaireId.FormatGuid();
            
            return interviewPageModel;
        }
    }

    public class QuestionnaireAttachment
    {
        public QuestionnaireAttachment(Guid id, AttachmentContent content)
        {
            Content = content;
            Id = id;
        }

        public Guid Id { get; set; }
        public AttachmentContent Content { get; set; }
    }

    public class InterviewPageModel
    {
        public string? Title { get; set; }
        public string? GoogleMapsKey { get; set; }
        public string? Id { get; set; }
        public string? CoverPageId { get; set; }
        public string? ReloadQuestionnaireUrl { get; set; }
        public string? OriginalInterviewId { get; set; }
        public string? SaveScenarioUrl { get; set; }
        public string? GetScenarioUrl { get; set; }
        public int? ScenarioId { get; set; }
        public string? DesignerUrl { get; set; }
        public string? QuestionnaireId { get; set; }
    }

    public class ApiTestModel
    {
        public ApiTestModel()
        {
            Attaches = new List<string>();
        }

        public Guid Id { get; set; }
        public DateTime LastUpdated { get; set; }
        public int NumOfTranslations { get; set; }
        public List<string> Attaches { get; set; }
        public string? Title { get; set; }
    }
}
