using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions.ReusableCategories;
using WB.Infrastructure.Native.Questionnaire;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.DataCollection.Enumerator.v1
{
    [Authorize(Roles = "Interviewer, Supervisor")]
    public class ReusableCategoriesApiV1Controller : ControllerBase
    {
        private readonly IReusableCategoriesStorage reusableCategoriesStorage;
        private readonly IQuestionnaireStorage questionnaireStorage;

        public ReusableCategoriesApiV1Controller(IReusableCategoriesStorage reusableCategoriesStorage, IQuestionnaireStorage questionnaireStorage)
        {
            this.reusableCategoriesStorage = reusableCategoriesStorage;
            this.questionnaireStorage = questionnaireStorage;
        }

        [HttpGet]
        [Route("api/enumerator/v1/categories/{id}")]
        [WriteToSyncLog(SynchronizationLogType.DownloadReusableCategories)]
        public virtual ActionResult<List<ReusableCategoriesDto>> Get(string id)
        {
            var questionnaireIdentity = QuestionnaireIdentity.Parse(id);
            var questionnaireDocument = this.questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity);

            List<ReusableCategoriesDto> reusableCategoriesDtos = questionnaireDocument.Categories.Select(c => new ReusableCategoriesDto()
            {
                Id = c.Id,
                Options = this.reusableCategoriesStorage.GetOptions(questionnaireIdentity, c.Id).ToList()
            }).ToList();

            return reusableCategoriesDtos;
        }
    }
}
