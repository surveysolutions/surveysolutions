using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.UI.Designer.Code;
using WB.UI.Designer.Models;
using WB.UI.Designer.Services;

namespace WB.UI.Designer.Controllers.Api.Designer
{
    [AuthorizeOrAnonymousQuestionnaire]
    [QuestionnairePermissions]
    [ResponseCache(NoStore = true)]
    [Route("api/questionnaire")]
    public class QuestionnaireApiController : Controller
    {
        private readonly IVerificationErrorsMapper verificationErrorsMapper;
        private readonly IQuestionnaireVerifier questionnaireVerifier;
        private readonly IQuestionnaireInfoFactory questionnaireInfoFactory;
        private readonly IOptions<WebTesterSettings> webTesterSettings;
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly IChapterInfoViewFactory chapterInfoViewFactory;
        private readonly IQuestionnaireInfoViewFactory questionnaireInfoViewFactory;
        private readonly IWebTesterService webTesterService;
        private readonly UserManager<DesignerIdentityUser> userManager;
        private readonly DesignerDbContext dbContext;
        private const int MaxCountOfOptionForFilteredCombobox = 200;
        public const int MaxVerificationErrorsOrWarnings = 100;

        public QuestionnaireApiController(IChapterInfoViewFactory chapterInfoViewFactory,
            IQuestionnaireInfoViewFactory questionnaireInfoViewFactory,
            IQuestionnaireViewFactory questionnaireViewFactory,
            IQuestionnaireVerifier questionnaireVerifier,
            IVerificationErrorsMapper verificationErrorsMapper,
            IQuestionnaireInfoFactory questionnaireInfoFactory,
            IOptions<WebTesterSettings> webTesterSettings,
            IWebTesterService webTesterService,
            UserManager<DesignerIdentityUser> userManager,
            DesignerDbContext dbContext)
        {
            this.chapterInfoViewFactory = chapterInfoViewFactory;
            this.questionnaireInfoViewFactory = questionnaireInfoViewFactory;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.questionnaireVerifier = questionnaireVerifier;
            this.verificationErrorsMapper = verificationErrorsMapper;
            this.questionnaireInfoFactory = questionnaireInfoFactory;
            this.webTesterSettings = webTesterSettings;
            this.webTesterService = webTesterService;
            this.userManager = userManager;
            this.dbContext = dbContext;
        }

        public IActionResult Details(string id)
        {
            return Ok();
        }

        [HttpGet]
        [Route("get/{id}")]
        public async Task<IActionResult> Get(QuestionnaireRevision? id)
        {
            if (id == null)
            {
                return NotFound(string.Format(ExceptionMessages.QuestionCannotBeFound , id));
            }

            var etag = await ComputeETagAsync(id);
            if (etag != null && (string?)Request.Headers.IfNoneMatch == etag)
            {
                Response.Headers.ETag = etag;
                return StatusCode(StatusCodes.Status304NotModified);
            }

            var questionnaireInfoView = this.questionnaireInfoViewFactory.Load(id, User.GetIdOrNull());

            if (questionnaireInfoView == null)
            {
                return NotFound(string.Format(ExceptionMessages.QuestionCannotBeFound , id));
            }

            if (etag != null)
            {
                Response.OnStarting(() =>
                {
                    Response.Headers.ETag = etag;
                    Response.Headers.CacheControl = "no-cache";
                    return Task.CompletedTask;
                });
            }

            return Ok(questionnaireInfoView);
        }

        [HttpGet]
        [Route("chapter/{id}")]
        public async Task<IActionResult> Chapter(QuestionnaireRevision id, string chapterId)
        {
            var etag = await ComputeETagAsync(id);
            if (etag != null && (string?)Request.Headers.IfNoneMatch == etag)
            {
                Response.Headers.ETag = etag;
                return StatusCode(StatusCodes.Status304NotModified);
            }

            var chapterInfoView = this.chapterInfoViewFactory.Load(id, chapterId: chapterId);

            if (chapterInfoView == null)
            {
                return NotFound();
            }

            if (etag != null)
            {
                Response.OnStarting(() =>
                {
                    Response.Headers.ETag = etag;
                    Response.Headers.CacheControl = "no-cache";
                    return Task.CompletedTask;
                });
            }

            return Ok(chapterInfoView);
        }

        private async Task<string?> ComputeETagAsync(QuestionnaireRevision id)
        {
            // A specific revision is an immutable snapshot — use its ID directly.
            if (id.Revision.HasValue)
                return $"\"{id.Revision:N}\"";

            // For the "latest" revision, derive the ETag from the highest change-record
            // sequence. Do not cache this value here: questionnaire changes can be saved
            // through multiple mutation paths, and correctness requires every one of them
            // to invalidate the cache entry.
            var latestSeq = await dbContext.QuestionnaireChangeRecords
                .Where(r => r.QuestionnaireId == id.QuestionnaireId.ToString("N"))
                .OrderByDescending(r => r.Sequence)
                .Select(r => r.Sequence)
                .FirstOrDefaultAsync();

            return latestSeq > 0 ? $"\"{id.QuestionnaireId:N}_{latestSeq}\"" : null;
        }

        [HttpGet]
        [Route("EditVariable/{id}")]
        public IActionResult EditVariable(QuestionnaireRevision id, Guid variableId)
        {
            var variableView = this.questionnaireInfoFactory.GetVariableEditView(id, variableId);

            if (variableView == null) return NotFound(string.Format(ExceptionMessages.VariableWithIdWasNotFound, variableId, id));

            var result = Ok(new
            {
                Id = variableView.ItemId,
                Expression = variableView.VariableData.Expression,
                variable = variableView.VariableData.Name,
                TypeOptions = variableView.TypeOptions,
                Type = variableView.VariableData.Type,
                breadcrumbs = variableView.Breadcrumbs,
                label = variableView.VariableData.Label,
                variableView.VariableData.DoNotExport
            });

            return result;
        }

        [HttpGet]
        [Route("EditQuestion/{id}")]
        public IActionResult EditQuestion(QuestionnaireRevision id, Guid questionId)
        {
            var editQuestionView = this.questionnaireInfoFactory.GetQuestionEditView(id, questionId);

            if (editQuestionView == null)
            {
                return NotFound();
            }
            
            if ((editQuestionView.IsFilteredCombobox == true || !string.IsNullOrWhiteSpace(editQuestionView.CascadeFromQuestionId))
                && editQuestionView.Options != null)
            {
                editQuestionView.WereOptionsTruncated = editQuestionView.Options.Length > MaxCountOfOptionForFilteredCombobox;
                editQuestionView.Options = editQuestionView.Options.Take(MaxCountOfOptionForFilteredCombobox).ToArray();   
            }

            return Ok(editQuestionView);
        }

        [HttpGet]
        [Route("EditGroup/{id}")]
        public IActionResult EditGroup(QuestionnaireRevision id, Guid groupId)
        {
            var editGroupView = this.questionnaireInfoFactory.GetGroupEditView(id, groupId);

            if (editGroupView == null)
            {
                return NotFound();
            }

            return Ok(editGroupView);
        }

        [HttpGet]
        [Route("EditRoster/{id}")]
        public IActionResult EditRoster(QuestionnaireRevision id, Guid rosterId)
        {
            var editRosterView = this.questionnaireInfoFactory.GetRosterEditView(id, rosterId);
            if (editRosterView == null)
            {
                return NotFound();
            }

            return Ok(editRosterView);
        }

        [HttpGet]
        [Route("EditStaticText/{id}")]
        public IActionResult EditStaticText(QuestionnaireRevision id, Guid staticTextId)
        {
            var staticTextEditView = this.questionnaireInfoFactory.GetStaticTextEditView(id, staticTextId);

            if (staticTextEditView == null)
            {
                return NotFound();
            }

            return Ok(staticTextEditView);
        }

        [HttpGet]
        [Route("Verify/{id}")]
        public IActionResult Verify(QuestionnaireRevision id)
        {
            var questionnaireView = this.questionnaireViewFactory.Load(id);

            if (questionnaireView == null)
            {
                return NotFound();
            }

            QuestionnaireVerificationMessage[] verificationMessagesAndWarning = 
                this.questionnaireVerifier.GetAllErrors(questionnaireView,true).ToArray();
            
            var verificationErrors = verificationMessagesAndWarning
                .Where(x => x.MessageLevel > VerificationMessageLevel.Warning)
                .Take(MaxVerificationErrorsOrWarnings)
                .ToArray();

            var verificationWarnings = verificationMessagesAndWarning
                .Where(x => x.MessageLevel == VerificationMessageLevel.Warning)
                .Take(MaxVerificationErrorsOrWarnings)
                .ToArray();

            var readOnlyQuestionnaire = new ReadOnlyQuestionnaireDocument(questionnaireView.Source);
            VerificationMessage[] errors = this.verificationErrorsMapper.EnrichVerificationErrors(verificationErrors, readOnlyQuestionnaire);
            VerificationMessage[] warnings = this.verificationErrorsMapper.EnrichVerificationErrors(verificationWarnings, readOnlyQuestionnaire);

            return Ok(new VerificationResult
            (
                errors : errors,
                warnings : warnings
            ));
        }

        /// <summary>
        /// Initiates a WebTester session for the specified questionnaire using a
        /// <b>one-time code exchange flow</b>. The JWT never reaches the browser.
        /// </summary>
        /// <remarks>
        /// <para><b>Flow:</b></para>
        /// <list type="number">
        ///   <item>Designer creates a short-lived, single-use code scoped to the questionnaire and user.</item>
        ///   <item>Returns a WebTester URL of the form <c>{BaseUri}/{id}?code=&lt;code&gt;</c>.</item>
        ///   <item>The browser is navigated to that URL (client-side redirect, not a server redirect).</item>
        ///   <item>WebTester's <c>Run</c> action receives the code, calls Designer's
        ///         <c>POST /api/internal/auth/exchange</c> <b>server-to-server</b>, and receives
        ///         a short-lived delegated JWT.</item>
        ///   <item>The JWT is stored server-side in <c>IWebTesterJwtStore</c> and attached to
        ///         outbound Designer API calls — it is never exposed to the browser.</item>
        /// </list>
        /// <para>
        /// <b>Note:</b> this endpoint does NOT return an <c>X-WebTester-Token</c> header,
        /// and the frontend does NOT append <c>?jwt=</c> to the URL.
        /// Those patterns belong to an earlier design and are no longer used.
        /// </para>
        /// </remarks>
        /// <returns>
        /// <c>200 OK</c> with the WebTester redirect URL as a plain string,
        /// e.g. <c>https://webtester.example.com/{id}?code=&lt;one-time-code&gt;</c>.
        /// </returns>
        [HttpGet]
        [Route("WebTest/{id:guid}")]
        public async Task<IActionResult> WebTest(Guid id)
        {
            if (string.IsNullOrWhiteSpace(webTesterSettings.Value.JwtSecretKey))
                return StatusCode(406, new
                {
                    error = "WebTester is not configured",
                    message = "WebTester:JwtSecretKey must be set in application configuration."
                });

            var userId = User.GetIdOrNull();
            DesignerIdentityUser? user = userId.HasValue
                ? await userManager.FindByIdAsync(userId.Value.ToString())
                : null;

            var correlationId = Guid.NewGuid().ToString("N");

            // Returns a one-time code in the URL — JWT never leaves the server.
            var code = await webTesterService.CreateOneTimeCodeAsync(
                id,
                user?.Id.ToString(),
                correlationId);

            var redirectUrl = $"{webTesterSettings.Value.BaseUri}/{id}?code={Uri.EscapeDataString(code)}";
            return Ok(redirectUrl);
        }

        [HttpGet]
        [Route("GetAllBrokenGroupDependencies/{id}")]
        public List<QuestionnaireItemLink> GetAllBrokenGroupDependencies(QuestionnaireRevision id, Guid groupId)
        {
            return this.questionnaireInfoFactory.GetAllBrokenGroupDependencies(id, groupId);
        }

        [HttpGet]
        [Route("GetQuestionsEligibleForNumericRosterTitle/{id}")]
        public List<DropdownEntityView>? GetQuestionsEligibleForNumericRosterTitle(QuestionnaireRevision id, Guid rosterId, Guid rosterSizeQuestionId)
        {
            return this.questionnaireInfoFactory.GetQuestionsEligibleForNumericRosterTitle(id, rosterId, rosterSizeQuestionId);
        }
    }
}
