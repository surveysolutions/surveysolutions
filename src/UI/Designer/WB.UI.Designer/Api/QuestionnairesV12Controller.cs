using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.UI.Designer.Api.Attributes;

namespace WB.UI.Designer.Api
{
    [ApiBasicAuth]
    [RoutePrefix("api/v12/questionnaires")]
    public class QuestionnairesV12Controller : ApiController
    {
        [Route("~/api/v12/login")]
        [HttpGet]
        public void Login()
        {
            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.UpgradeRequired));
        }

        [Route("{id:Guid}")]
        public Questionnaire Get(Guid id)
        {
            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.UpgradeRequired));
        }

        [Route("")]
        public IEnumerable<QuestionnaireListItem> Get([FromUri]int pageIndex = 1, [FromUri]int pageSize = 128, [FromUri]string sortBy = "", [FromUri]string filter = "", [FromUri]bool isPublic = false)
        {
            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.UpgradeRequired));
        }
    }
}