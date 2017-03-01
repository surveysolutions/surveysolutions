using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Prometheus;
using Prometheus.Advanced;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api
{
    public class MetricsController : ApiController
    {
        public HttpResponseMessage Get()
        {
            using (var ms = new MemoryStream())
            {
                ScrapeHandler.ProcessScrapeRequest(DefaultCollectorRegistry.Instance.CollectAll(), @"text/plain", ms);

                var resp = new HttpResponseMessage(HttpStatusCode.OK);
                resp.Content = new StringContent(Encoding.UTF8.GetString(ms.ToArray()), Encoding.UTF8, @"text/plain");
                return resp;
            }
        }
    }
}