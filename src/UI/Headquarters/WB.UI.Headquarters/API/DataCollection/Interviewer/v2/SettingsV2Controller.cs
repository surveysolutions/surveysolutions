using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Models.CompanyLogo;

namespace WB.UI.Headquarters.API.DataCollection.Interviewer.v2
{
    [ApiBasicAuth(new[] { UserRoles.Interviewer })]
    public class SettingsV2Controller : ApiController
    {
        private readonly IPlainKeyValueStorage<CompanyLogo> appSettingsStorage;
        private readonly IPlainKeyValueStorage<InterviewerSettings> interviewerSettingsStorage;

        public SettingsV2Controller(IPlainKeyValueStorage<CompanyLogo> appSettingsStorage, 
            IPlainKeyValueStorage<InterviewerSettings> interviewerSettingsStorage)
        {
            this.appSettingsStorage = appSettingsStorage;
            this.interviewerSettingsStorage = interviewerSettingsStorage;
        }

        [HttpGet]
        public HttpResponseMessage CompanyLogo()
        {
            var incomingEtag = Request.Headers.IfNoneMatch.FirstOrDefault()?.Tag ?? "";
            var companyLogo = this.appSettingsStorage.GetById(AppSetting.CompanyLogoStorageKey);

            if (companyLogo == null) return Request.CreateResponse(HttpStatusCode.NoContent);

            var etagValue = companyLogo.GetEtagValue();
            if (etagValue.Equals(incomingEtag.Replace("\"", ""), StringComparison.InvariantCultureIgnoreCase))
            {
                return Request.CreateResponse(HttpStatusCode.NotModified);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(companyLogo.Logo);
            response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(@"image/png");
            response.Headers.ETag = new EntityTagHeaderValue($"\"{etagValue}\"");

            return response;
        }

        [HttpGet]
        public bool AutoUpdateEnabled() =>
            this.interviewerSettingsStorage.GetById(AppSetting.InterviewerSettings)?.AutoUpdateEnabled ?? true;
    }
}
