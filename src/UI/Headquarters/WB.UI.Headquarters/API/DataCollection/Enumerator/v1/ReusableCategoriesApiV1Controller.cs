using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions.ReusableCategories;
using WB.Infrastructure.Native.Questionnaire;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.DataCollection.Enumerator.v1
{
    [ApiBasicAuth(new[] { UserRoles.Interviewer, UserRoles.Supervisor })]

    public class ReusableCategoriesApiV1Controller : ApiController
    {
        private readonly IReusableCategoriesStorage reusableCategoriesStorage;
        private readonly IQuestionnaireStorage questionnaireStorage;

        protected ReusableCategoriesApiV1Controller(IReusableCategoriesStorage reusableCategoriesStorage, IQuestionnaireStorage questionnaireStorage)
        {
            this.reusableCategoriesStorage = reusableCategoriesStorage;
            this.questionnaireStorage = questionnaireStorage;
        }

        [HttpGet]
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
