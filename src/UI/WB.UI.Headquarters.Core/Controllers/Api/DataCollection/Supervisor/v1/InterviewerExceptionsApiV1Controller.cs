using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Supervisor.v1
{
    [Authorize(Roles = "Supervisor")]
    public class InterviewerExceptionsApiV1Controller : ControllerBase
    {
        private readonly IPlainStorageAccessor<SynchronizationLogItem> syncLogRepository;

        public InterviewerExceptionsApiV1Controller(IPlainStorageAccessor<SynchronizationLogItem> syncLogRepository)
        {
            this.syncLogRepository = syncLogRepository;
        }

        [HttpPost]
        [Route("api/supervisor/v1/interviewerExceptions")]
        public IActionResult Post([FromBody]List<InterviewerExceptionInfo> exceptions)
        {
            foreach (var exception in exceptions)
            {
                this.syncLogRepository.Store(new SynchronizationLogItem
                {
                    InterviewerId = exception.InterviewerId,
                    LogDate = DateTime.UtcNow,
                    Type = SynchronizationLogType.DeviceUnexpectedException,
                    Log = $@"<pre><font color=""red"">{exception.StackTrace.Replace("\r\n", "<br />")}</font></pre>"
                }, Guid.NewGuid());
            }

            return this.Ok();
        }
    }
}
