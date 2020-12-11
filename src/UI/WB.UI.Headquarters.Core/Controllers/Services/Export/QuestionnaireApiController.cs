using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Infrastructure.Native.Questionnaire;
using WB.UI.Headquarters.Code.Authentication;

namespace WB.UI.Headquarters.Controllers.Services.Export
{
    [Localizable(false)]
    [Route("api/export/v1/questionnaire")]
    [Authorize(AuthenticationSchemes = AuthType.TenantToken)]
    public class QuestionnaireApiController : Controller
    {
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly ISerializer serializer;
        private readonly IPlainKeyValueStorage<QuestionnairePdf> pdfStorage;
        private readonly IPlainKeyValueStorage<QuestionnaireBackup> questionnaireBackupStorage;
        private readonly IReusableCategoriesStorage reusableCategoriesStorage;
        private readonly ITranslationStorage translationStorage;
        private readonly IQuestionnaireTranslator translator;
        
        public QuestionnaireApiController(IQuestionnaireStorage questionnaireStorage,
            ISerializer serializer,
            IPlainKeyValueStorage<QuestionnairePdf> pdfStorage,
            IReusableCategoriesStorage reusableCategoriesStorage,
            ITranslationStorage translationStorage,
            IQuestionnaireTranslator translator, IPlainKeyValueStorage<QuestionnaireBackup> questionnaireBackupStorage)
        {
            this.questionnaireStorage = questionnaireStorage ?? throw new ArgumentNullException(nameof(questionnaireStorage));
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            this.pdfStorage = pdfStorage ?? throw new ArgumentNullException(nameof(pdfStorage));
            this.reusableCategoriesStorage = reusableCategoriesStorage ?? throw new ArgumentNullException(nameof(reusableCategoriesStorage));
            this.translationStorage = translationStorage ?? throw new ArgumentNullException(nameof(translationStorage));
            this.translator = translator ?? throw new ArgumentNullException(nameof(translator));
            this.questionnaireBackupStorage = questionnaireBackupStorage;
        }

        [Route("{id}")]
        [HttpGet]
        public ActionResult Get(string id, [FromQuery] Guid? translation = null)
        {
            var questionnaireIdentity = QuestionnaireIdentity.Parse(id);
            var questionnaireDocument = this.questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity);
            if (translation != null)
            {
                var translationInstance = this.translationStorage.Get(questionnaireIdentity, translation.Value);
                questionnaireDocument = this.translator.Translate(questionnaireDocument, translationInstance);
            }
            
            var content = this.serializer.Serialize(questionnaireDocument);
            return Content(content, "application/json");
        }

        [Route("{id}/pdf")]
        [HttpGet]
        public ActionResult Pdf(string id, [FromQuery]Guid? translation = null)
        {
            QuestionnairePdf pdf = 
                this.pdfStorage.GetById(translation.HasValue ? $"{translation.FormatGuid()}_{id}" : id);

            if (pdf == null) return NotFound("Questionnaire not found");

            return File(pdf.Content, "application/pdf");
        }

        [Route("{id}/category/{categoryId}")]
        [HttpGet]
        public ActionResult<List<CategoriesItem>> Category(string id, Guid categoryId, [FromQuery] Guid? translation = null)
        {
            var questionnaireIdentity = QuestionnaireIdentity.Parse(id);
            var categoriesItems = reusableCategoriesStorage.GetOptions(questionnaireIdentity, categoryId);
            if (categoriesItems == null) 
                return NotFound();
            var categoriesList = categoriesItems.ToList();
            if (translation.HasValue)
            {
                ITranslation translationInstance = this.translationStorage.Get(questionnaireIdentity, translation.Value);
                foreach (var category in categoriesList)
                {
                    category.Text = translationInstance.GetCategoriesText(categoryId, category.Id, category.ParentId)
                        ?? category.Text;
                }
            }

            return categoriesList;
        }

        [Route("{id}/backup")]
        [HttpGet]
        public ActionResult Backup(string id)
        {
            var backup = questionnaireBackupStorage.GetById(id);
            if (backup == null) return NotFound("Questionnaire backup not found.");

            return File(backup.Content, "application/zip");
        }
    }
}
