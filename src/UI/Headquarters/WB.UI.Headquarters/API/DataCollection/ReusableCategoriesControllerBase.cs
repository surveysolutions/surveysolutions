using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.SurveySolutions.ReusableCategories;

namespace WB.UI.Headquarters.API.DataCollection
{
    public abstract class ReusableCategoriesControllerBase : ApiController
    {
        private readonly IReusableCategoriesStorage reusableCategoriesStorage;
        private readonly IQuestionnaireStorage questionnaireStorage;

        protected ReusableCategoriesControllerBase(IReusableCategoriesStorage reusableCategoriesStorage, IQuestionnaireStorage questionnaireStorage)
        {
            this.reusableCategoriesStorage = reusableCategoriesStorage;
            this.questionnaireStorage = questionnaireStorage;
        }

        public virtual HttpResponseMessage Get(string id)
        {
            var questionnaireIdentity = QuestionnaireIdentity.Parse(id);
            var questionnaireDocument = this.questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity);

            var reusableCategoriesDtos = questionnaireDocument.Categories.Select(c => new ReusableCategoriesDto()
            {
                Id = c.Id,
                Options = this.reusableCategoriesStorage.GetOptions(questionnaireIdentity, c.Id).ToList()
            });

            return Request.CreateResponse(HttpStatusCode.OK, reusableCategoriesDtos);
        }
    }
}
