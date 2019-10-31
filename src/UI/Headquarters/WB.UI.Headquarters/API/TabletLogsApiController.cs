using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Utils;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Models.Api;
using WB.UI.Shared.Web.Attributes;
using WB.UI.Shared.Web.Extensions;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.API
{
    [Authorize(Roles = "Administrator")]
    [Localizable(false)]
    public class TabletLogsApiController : ApiController
    {
        private readonly IPlainStorageAccessor<TabletLog> logs;

        public TabletLogsApiController(IPlainStorageAccessor<TabletLog> logs)
        {
            this.logs = logs;
        }

        [HttpGet]
        [CamelCase]
        public IHttpActionResult Table([FromUri]DataTableRequest request)
        {
            var totalCount = logs.Query(_ => _.Count());
            var result = logs.Query(_ => _
                .OrderUsingSortExpression(request.GetSortOrder())
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList()
                .Select(x => new LogRow
                {
                    Id = x.Id,
                    DeviceId = x.DeviceId,
                    ReceiveDateUtc = x.ReceiveDateUtc,
                    UserName = x.UserName,
                    DownloadUrl = Url.Route("DefaultApiWithAction",
                        new
                        {
                            Action = "Download",
                            id = x.Id
                        })
                }).ToList());

            var response = new DataTableResponse<LogRow>
            {
                Draw = request.Draw + 1,
                RecordsTotal = totalCount,
                RecordsFiltered = totalCount,
                Data = result
            };
            return this.Ok(response);
        }

        [HttpGet]
        [ApiNoCache]
        public HttpResponseMessage Download(int id)
        {
            var log = logs.GetById(id);
            if (log == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var result = Request.AsProgressiveDownload(new MemoryStream(log.Content), 
                "application/zip",
                $"Logs {log.UserName} {log.ReceiveDateUtc:s}.zip");
            return result;
        }

        public struct LogRow
        {
            public int Id { get; set; }

            public string DeviceId { get; set; }

            public string UserName { get; set; }

            public DateTime ReceiveDateUtc { get; set; }

            public string DownloadUrl { get; set; }
        }
    }
}
