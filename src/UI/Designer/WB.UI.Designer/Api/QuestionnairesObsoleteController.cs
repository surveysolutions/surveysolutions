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
    public class QuestionnairesObsoleteController : ApiController
    {
        [Route("api/v13/login")]
        [Route("api/v12/login")]
        [Route("api/v11/login")]
        [Route("api/v10/login")]
        [Route("api/v9/login")]
        [Route("api/v8/login")]
        [HttpGet]
        public void Login()
        {
            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.UpgradeRequired));
        }

        [Route("api/v13/questionnaires/{id:Guid}")]
        [Route("api/v12/questionnaires/{id:Guid}")]
        [Route("api/v11/questionnaires/{id:Guid}")]
        [Route("api/v10/questionnaires/{id:Guid}")]
        [Route("api/v9/questionnaires/{id:Guid}")]
        [Route("api/v8/questionnaires/{id:Guid}")]
        public Questionnaire Get(Guid id)
        {
            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.UpgradeRequired));
        }

        [Route("api/v13/questionnaires")]
        [Route("api/v12/questionnaires")]
        [Route("api/v11/questionnaires")]
        [Route("api/v10/questionnaires")]
        [Route("api/v9/questionnaires")]
        [Route("api/v8/questionnaires")]
        public IEnumerable<QuestionnaireListItem> Get([FromUri]int pageIndex = 1, [FromUri]int pageSize = 128, [FromUri]string sortBy = "", [FromUri]string filter = "", [FromUri]bool isPublic = false)
        {
            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.UpgradeRequired));
        }
    }
}