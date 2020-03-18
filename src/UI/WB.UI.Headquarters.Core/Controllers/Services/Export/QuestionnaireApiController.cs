using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Infrastructure.Native.Questionnaire;

namespace WB.UI.Headquarters.Controllers.Services.Export
{
    [Localizable(false)]
    [Route("api/export/v1/questionnaire")]
    public class QuestionnaireApiController : Controller
    {
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly ISerializer serializer;
        private readonly IPlainKeyValueStorage<QuestionnairePdf> pdfStorage;
        private readonly IReusableCategoriesStorage reusableCategoriesStorage;

        public QuestionnaireApiController(IQuestionnaireStorage questionnaireStorage, ISerializer serializer, IPlainKeyValueStorage<QuestionnairePdf> pdfStorage,
            IReusableCategoriesStorage reusableCategoriesStorage)
        {
            this.questionnaireStorage = questionnaireStorage ?? throw new ArgumentNullException(nameof(questionnaireStorage));
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            this.pdfStorage = pdfStorage ?? throw new ArgumentNullException(nameof(pdfStorage));
            this.reusableCategoriesStorage = reusableCategoriesStorage;
        }

        [Route("{id}")]
        [ServiceApiKeyAuthorization]
        [HttpGet]
        public ActionResult Get(string id)
        {
            var questionnaireIdentity = QuestionnaireIdentity.Parse(id);
            var questionnaireDocument = this.questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity);
            var content = this.serializer.Serialize(questionnaireDocument);// , Encoding.UTF8, "application/json");
            return Content(content, "application/json");
        }

        [Route("{id}/pdf")]
        [ServiceApiKeyAuthorization]
        [HttpGet]
        public ActionResult Pdf(string id, [FromQuery]Guid? translation = null)
        {
            QuestionnairePdf pdf = 
                this.pdfStorage.GetById(translation.HasValue ? $"{translation.FormatGuid()}_{id}" : id);

            if (pdf == null) return NotFound("Questionnaire not found");

            return File(pdf.Content, "application/pdf");
        }

        [Route("{id}/category/{categoryId}")]
        [ServiceApiKeyAuthorization]
        [HttpGet]
        public ActionResult<List<CategoriesItem>> Category(string id, Guid categoryId)
        {
            var questionnaireIdentity = QuestionnaireIdentity.Parse(id);
            var categoriesItems = reusableCategoriesStorage.GetOptions(questionnaireIdentity, categoryId);

            if (categoriesItems == null) return NotFound();

            return categoriesItems.ToList();
        }
    }
}
