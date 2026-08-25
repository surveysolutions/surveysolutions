using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Designer;
using WB.UI.Designer.Code.ImportExport;

namespace WB.UI.Designer.Controllers.Api.Assistant
{
    /// <summary>
    /// Back-channel API used by the Assistant provider while importing a questionnaire produced from
    /// an arbitrary source file. Designer stays the source of truth for both the JSON schema and the
    /// Survey Solutions domain rules.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/v1/assistant/questionnaire-import")]
    public class QuestionnaireImportController : ControllerBase
    {
        private const int MaxQuestionnaireJsonLength = 20 * 1024 * 1024;

        private readonly IQuestionnaireSchemaProvider schemaProvider;
        private readonly IQuestionnaireJsonImportService importService;
        private readonly ILogger<QuestionnaireImportController> logger;

        public QuestionnaireImportController(
            IQuestionnaireSchemaProvider schemaProvider,
            IQuestionnaireJsonImportService importService,
            ILogger<QuestionnaireImportController> logger)
        {
            this.schemaProvider = schemaProvider;
            this.importService = importService;
            this.logger = logger;
        }

        public class ValidateAndImportRequest
        {
            [Required]
            [StringLength(MaxQuestionnaireJsonLength)]
            public string QuestionnaireJson { get; set; } = string.Empty;
        }

        public class SchemaResponse
        {
            public string Version { get; set; } = string.Empty;
            public string Schema { get; set; } = string.Empty;
        }

        public class ValidateAndImportResponse
        {
            public bool Succeeded { get; set; }
            public Guid? QuestionnaireId { get; set; }
            public string? QuestionnaireTitle { get; set; }
            public string SchemaVersion { get; set; } = string.Empty;
            public List<string> Errors { get; set; } = new List<string>();
            public List<QuestionnaireJsonImportWarning> Warnings { get; set; } = new List<QuestionnaireJsonImportWarning>();
        }

        [HttpGet]
        [Route("schema")]
        [Produces(MediaTypeNames.Application.Json)]
        public ActionResult<SchemaResponse> GetSchema()
            => new SchemaResponse
            {
                Version = schemaProvider.GetSchemaVersion(),
                Schema = schemaProvider.GetSchema()
            };

        /// <summary>
        /// Validates the questionnaire against the questionnaire schema and the Survey Solutions domain
        /// rules and creates it only when validation succeeds.
        /// </summary>
        [HttpPost]
        [RequestSizeLimit(MaxQuestionnaireJsonLength)]
        public ActionResult<ValidateAndImportResponse> ValidateAndImport([FromBody] ValidateAndImportRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.QuestionnaireJson))
                return BadRequest("'questionnaireJson' must be provided.");

            var responsibleId = User.GetId();

            try
            {
                var result = importService.ValidateAndImport(request.QuestionnaireJson, responsibleId);

                logger.LogInformation(
                    "Assistant questionnaire import for user {UserId}: succeeded={Succeeded}, schemaVersion={SchemaVersion}, errors={ErrorCount}, warnings={WarningCount}",
                    responsibleId, result.Succeeded, result.SchemaVersion, result.Errors.Count, result.Warnings.Count);

                return new ValidateAndImportResponse
                {
                    Succeeded = result.Succeeded,
                    QuestionnaireId = result.QuestionnaireId,
                    QuestionnaireTitle = result.QuestionnaireTitle,
                    SchemaVersion = result.SchemaVersion,
                    Errors = result.Errors,
                    Warnings = result.Warnings
                };
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unexpected error while importing an assistant-generated questionnaire.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ValidateAndImportResponse
                    {
                        SchemaVersion = schemaProvider.GetSchemaVersion(),
                        // Admin-only back channel: the message is needed to diagnose a failed import.
                        Errors = { $"Designer failed to import the questionnaire: {exception.GetType().Name}: {exception.Message}" }
                    });
            }
        }
    }
}
