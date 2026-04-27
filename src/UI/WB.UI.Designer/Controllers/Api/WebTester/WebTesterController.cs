using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Api;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.UI.Designer.Api.WebTester;
using WB.UI.Designer.Controllers.Api.Designer;
using WB.UI.Designer.Services;

namespace WB.UI.Designer.Controllers.Api.WebTester
{
    [ApiController]
    [Route("api/webtester")]
    [Authorize(AuthenticationSchemes = DelegatedTokenService.DelegatedScheme)]
    public class WebTesterController : ControllerBase
    {
        private readonly IQuestionnairePackageComposer questionnairePackageComposer;

        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly IAttachmentService attachmentService;
        private readonly DesignerDbContext designerDbContext;
        private readonly ISerializer serializer;

        public WebTesterController(
            IQuestionnairePackageComposer questionnairePackageComposer,
            IQuestionnaireViewFactory questionnaireViewFactory,
            IAttachmentService attachmentService,
            DesignerDbContext designerDbContext,
            ISerializer serializer)
        {
            this.questionnairePackageComposer = questionnairePackageComposer;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.attachmentService = attachmentService;
            this.designerDbContext = designerDbContext;
            this.serializer = serializer;
        }

        [Route("{questionnaireId:Guid}/info")]
        [HttpGet]
        public IActionResult Info(Guid questionnaireId)
        {
            if (!User.HasMatchingQuestionnaireId(questionnaireId))
                return Forbid();

            var questionnaireView = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(questionnaireId));
            if (questionnaireView == null)
            {
                return NotFound();
            }

            return Ok(new QuestionnaireLiteInfo
            {
                Id = questionnaireView.PublicKey,
                LastUpdateDate = questionnaireView.Source.LastEntryDate
            });
        }
        
        [Route("{questionnaireId:Guid}/settings")]
        [HttpGet]
        public IActionResult Settings(Guid questionnaireId)
        {
            if (!User.HasMatchingQuestionnaireId(questionnaireId))
                return Forbid();
            
            var anonymousQuestionnaire = this.designerDbContext.AnonymousQuestionnaires.FirstOrDefault(a =>
                a.AnonymousQuestionnaireId == questionnaireId && a.IsActive == true);

            return Ok(new QuestionnaireSettings
            {
                IsAnonymousMode = anonymousQuestionnaire == null
            });
        }

        [Route("{questionnaireId:Guid}/questionnaire")]
        [HttpGet]
        public IActionResult QuestionnaireAsync(Guid questionnaireId)
        {
            if (!User.HasMatchingQuestionnaireId(questionnaireId))
                return Forbid();

            try
            {
                var composeQuestionnaire = this.questionnairePackageComposer.ComposeQuestionnaire(questionnaireId);
                if(composeQuestionnaire == null)
                    return NotFound();
                
                return Ok(this.serializer.Serialize(composeQuestionnaire));
            }
            catch (ComposeException )
            {
                return StatusCode((int) HttpStatusCode.PreconditionFailed);
            }
        }

        private Guid GetOriginalQuestionnaireId([DisallowNull] Guid? questionnaireId)
        {
            var anonymousQuestionnaire = designerDbContext.AnonymousQuestionnaires.FirstOrDefault(a => a.IsActive == true
                && a.AnonymousQuestionnaireId == questionnaireId.Value);

            var qId = anonymousQuestionnaire?.QuestionnaireId ?? questionnaireId.Value;
            return qId;
        }

        [Route("{questionnaireId:Guid}/attachment/{attachmentContentId}")]
        [HttpGet]
        public IActionResult AttachmentContentAsync(Guid questionnaireId, string attachmentContentId)
        {
            if (!User.HasMatchingQuestionnaireId(questionnaireId))
                return Forbid();

            var qId = GetOriginalQuestionnaireId(questionnaireId);

            if (this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(qId)) == null)
            {
                return NotFound();
            }

            // IDOR guard: verify the requested attachment belongs to this questionnaire
            var questionnaireAttachments = this.attachmentService.GetAttachmentsByQuestionnaire(qId);
            var attachmentBelongsToQuestionnaire = questionnaireAttachments
                .Any(a => string.Equals(a.ContentId, attachmentContentId, StringComparison.OrdinalIgnoreCase));
            if (!attachmentBelongsToQuestionnaire)
                return Forbid();

            var attachmentContent = this.attachmentService.GetContent(attachmentContentId);

            if (attachmentContent == null)
                return NotFound();

            return Ok(new AttachmentContent
            {
                ContentType = attachmentContent.ContentType,
                Content = attachmentContent.Content,
                Id = attachmentContent.ContentId,
                Size = attachmentContent.Size
            });
        }

        [Route("{questionnaireId:Guid}/translations")]
        [HttpGet]
        public IActionResult TranslationsAsync(Guid questionnaireId)
        {
            if (!User.HasMatchingQuestionnaireId(questionnaireId))
                return Forbid();

            var qId = GetOriginalQuestionnaireId(questionnaireId);

            var questionnaireView = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(qId));
            if (questionnaireView == null) return NotFound();

            var actualTranslations = questionnaireView.Source.Translations.Select(x => x.Id).ToList();

            var model = this.designerDbContext.TranslationInstances
                .Where(x => x.QuestionnaireId == qId && actualTranslations.Contains(x.TranslationId))
                .Select(x => new TranslationDto
                {
                    Value = x.Value,
                    Type = x.Type,
                    TranslationId = x.TranslationId,
                    QuestionnaireEntityId = x.QuestionnaireEntityId,
                    TranslationIndex = x.TranslationIndex
                })
                .ToArray();
            return Ok(model);
        }

        [Route("{questionnaireId:Guid}/categories")]
        [HttpGet]
        public IActionResult CategoriesAsync(Guid questionnaireId)
        {
            if (!User.HasMatchingQuestionnaireId(questionnaireId))
                return Forbid();

            var qId = GetOriginalQuestionnaireId(questionnaireId);

            var questionnaireView = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(qId));
            if (questionnaireView == null) return NotFound();

            var actualCategories = questionnaireView.Source.Categories.Select(x => x.Id).ToList();

            var model = this.designerDbContext.CategoriesInstances
                .Where(x => x.QuestionnaireId == qId && actualCategories.Contains(x.CategoriesId))
                .OrderBy(x => x.SortIndex)
                .Select(x => new CategoriesDto
                {
                    CategoriesId = x.CategoriesId,
                    Id = x.Value,
                    ParentId = x.ParentId,
                    Text = x.Text,
                    AttachmentName = x.AttachmentName
                })
                .ToArray();

            return Ok(model);
        }
    }
}
