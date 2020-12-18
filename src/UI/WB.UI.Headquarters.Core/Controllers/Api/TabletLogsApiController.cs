using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Utils;
using WB.UI.Headquarters.Models.Api;

namespace WB.UI.Headquarters.Controllers.Api
{
    [Authorize(Roles = "Administrator")]
    [Localizable(false)]
    public class TabletLogsApiController : ControllerBase
    {
        private readonly IPlainStorageAccessor<TabletLog> logs;

        public TabletLogsApiController(IPlainStorageAccessor<TabletLog> logs)
        {
            this.logs = logs;
        }

        [HttpGet]
        public IActionResult Table(DataTableRequest request)
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
                    DownloadUrl = Url.Action("Download",
                        values: new
                        {
                            id = x.Id
                        })
                }).ToList());

            var response = new DataTableResponse<LogRow>
            {
                Draw = request.Draw,
                RecordsTotal = totalCount,
                RecordsFiltered = totalCount,
                Data = result
            };
            return this.Ok(response);
        }

        [HttpGet]
        public IActionResult Download(int id)
        {
            var log = logs.GetById(id);
            if (log == null)
                return NotFound();
            return File(new MemoryStream(log.Content),
                "application/zip",
                fileDownloadName: $"Logs {log.UserName} {log.ReceiveDateUtc:s}.zip");
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
