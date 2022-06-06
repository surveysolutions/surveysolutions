using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ImportExport;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.UI.Designer.Code;
using WB.UI.Designer.Code.ImportExport;
using WB.UI.Designer.Models;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Controllers.Api.Headquarters
{
    [Route("api/hq/backup")]
    [Authorize]
    public class HQQuestionnareBackupController : HQControllerBase
    {
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly IVerificationErrorsMapper verificationErrorsMapper;
        private readonly IQuestionnaireHelper questionnaireHelper;
        private readonly IQuestionnaireExportService questionnaireExportService;
        public HQQuestionnareBackupController(IQuestionnaireHelper questionnaireHelper, 
            IQuestionnaireViewFactory questionnaireViewFactory,
            IVerificationErrorsMapper verificationErrorsMapper,
            IQuestionnaireExportService questionnaireExportService)
        {
            this.questionnaireHelper = questionnaireHelper;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.verificationErrorsMapper = verificationErrorsMapper;
            this.questionnaireExportService = questionnaireExportService;
        }

        private const int MaxVerificationErrors = 20;

        [HttpGet]
        [Route("verify/{questionnaireRevision}")]
        public IActionResult Verify(QuestionnaireRevision questionnaireRevision)
        {
            var questionnaireView = this.questionnaireViewFactory.Load(questionnaireRevision);
            if (questionnaireView == null)
            {
                return this.ErrorWithReasonPhraseForHQ(StatusCodes.Status404NotFound, string.Format(ErrorMessages.TemplateNotFound, questionnaireRevision));
            }

            if (!ValidateAccessPermissionsOrAdmin(questionnaireView))
            {
                return this.ErrorWithReasonPhraseForHQ(StatusCodes.Status403Forbidden, ErrorMessages.NoAccessToQuestionnaire);
            }

            var result = VerifyQuestionnaire(questionnaireView);

            var verificationErrors = result
                .Where(x => x.MessageLevel > VerificationMessageLevel.Warning)
                .Take(MaxVerificationErrors)
                .ToArray();

            var verificationWarnings = result
                .Where(x => x.MessageLevel == VerificationMessageLevel.Warning)
                .Take(MaxVerificationErrors - verificationErrors.Length)
                .ToArray();

            var readOnlyQuestionnaire = questionnaireView.Source.AsReadOnly();
            VerificationMessage[] errors = this.verificationErrorsMapper.EnrichVerificationErrors(verificationErrors, readOnlyQuestionnaire);
            VerificationMessage[] warnings = this.verificationErrorsMapper.EnrichVerificationErrors(verificationWarnings, readOnlyQuestionnaire);

            return Ok(new VerificationResult
            (
                errors : errors,
                warnings : warnings
            ));
        }

        private static List<QuestionnaireVerificationMessage> VerifyQuestionnaire(QuestionnaireView questionnaireView)
        {
            var questionnaireDocument = questionnaireView.Source;
            var readOnlyQuestionnaireDocument = questionnaireDocument.AsReadOnly();

            var verifier = new ExportQuestionnaireVerifier();
            var result = verifier.Verify(readOnlyQuestionnaireDocument).ToList();
            return result;
        }

        [HttpGet]
        [Route("{questionnaireRevision}")]
        public IActionResult Get(Guid questionnaireRevision)
        {
            var questionnaireView = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(questionnaireRevision));
            if (questionnaireView == null)
            {
                return this.ErrorWithReasonPhraseForHQ(StatusCodes.Status404NotFound, string.Format(ErrorMessages.TemplateNotFound, questionnaireRevision));
            }

            if (!ValidateAccessPermissionsOrAdmin(questionnaireView))
            {
                return this.ErrorWithReasonPhraseForHQ(StatusCodes.Status403Forbidden, ErrorMessages.NoAccessToQuestionnaire);
            }

            var stream = this.questionnaireHelper.GetBackupQuestionnaire(questionnaireRevision, out string questionnaireFileName);
            if (stream == null) return NotFound();

            return File(stream, "application/zip", $"{questionnaireFileName}.zip");
        }
        
        [HttpGet]
        [Route("package/{questionnaireRevision}")]
        public IActionResult GetQuestionnairePackage(QuestionnaireRevision questionnaireRevision)
        {
            if (questionnaireRevision.OriginalQuestionnaireId.HasValue)
                return this.ErrorWithReasonPhraseForHQ(StatusCodes.Status404NotFound, string.Format(ErrorMessages.TemplateNotFound, questionnaireRevision.QuestionnaireId));
            
            var questionnaireView = this.questionnaireViewFactory.Load(questionnaireRevision);
            if (questionnaireView == null)
            {
                return this.ErrorWithReasonPhraseForHQ(StatusCodes.Status404NotFound, string.Format(ErrorMessages.TemplateNotFound, questionnaireRevision));
            }

            if (!ValidateAccessPermissionsOrAdmin(questionnaireView))
            {
                return this.ErrorWithReasonPhraseForHQ(StatusCodes.Status403Forbidden, ErrorMessages.NoAccessToQuestionnaire);
            }

            var result = VerifyQuestionnaire(questionnaireView);
            if (result.Any(v => v.MessageLevel > VerificationMessageLevel.Warning))
            {
                return Forbid();
            }

            var stream = this.questionnaireExportService.GetBackupQuestionnaire(questionnaireRevision, out string questionnaireFileName);
            if (stream == null) return NotFound();

            return File(stream, "application/zip", $"{questionnaireFileName}.zip");
        }
    }
}
