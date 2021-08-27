using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.UI.Designer.Controllers.Api.Tester
{
    [Authorize]
    [Route("api/v{version:int}/translation")]
    public class TranslationController : ControllerBase
    {
        private readonly DesignerDbContext dbContext;
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;

        public TranslationController(DesignerDbContext dbContext, IQuestionnaireViewFactory questionnaireViewFactory)
        {
            this.dbContext = dbContext;
            this.questionnaireViewFactory = questionnaireViewFactory;
        }

        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Get(Guid id, int version)
        {
            if (version < ApiVersion.CurrentTesterProtocolVersion)
                return StatusCode(StatusCodes.Status426UpgradeRequired);

            var questionnaireView = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));

            if(questionnaireView == null)
                return NotFound();

            var translationsIds = questionnaireView.Source.Translations.Select(x => x.Id).ToList();

            var translationInstances = await this.dbContext.TranslationInstances.Where(x => x.QuestionnaireId == id && translationsIds.Contains(x.TranslationId))
                                                     .ToListAsync();
            var result = translationInstances
                .Select(x => new TranslationDto
            {
                Value = x.Value,
                Type = x.Type,
                TranslationId = x.TranslationId,
                QuestionnaireEntityId = x.QuestionnaireEntityId,
                TranslationIndex = x.TranslationIndex
            }).ToArray();

            return Ok(result);
        }
    }
}
