using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Prometheus;
using Prometheus.Advanced;
using WB.UI.Headquarters.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api
{
    public class MetricsController : ApiController
    {
        public HttpResponseMessage Get()
        {
            if (Request.IsLocal() && MetricsService.IsEnabled)
            {
                using (var ms = new MemoryStream())
                {
                    ScrapeHandler.ProcessScrapeRequest(DefaultCollectorRegistry.Instance.CollectAll(), @"text/plain", ms);

                    var resp = new HttpResponseMessage(HttpStatusCode.OK);
                    resp.Content = new StringContent(Encoding.UTF8.GetString(ms.ToArray()), Encoding.UTF8, @"text/plain");
                    return resp;
                }
            }

            return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Not available");
        }
    }
}