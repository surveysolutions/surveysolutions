using System;
using System.Collections.Generic;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.DataCollection.Supervisor.v1
{
    [ApiBasicAuth(new[] { UserRoles.Supervisor })]
    public class InterviewerExceptionsApiV1Controller : ApiController
    {
        private readonly IPlainStorageAccessor<SynchronizationLogItem> syncLogRepository;

        public InterviewerExceptionsApiV1Controller(IPlainStorageAccessor<SynchronizationLogItem> syncLogRepository)
        {
            this.syncLogRepository = syncLogRepository;
        }

        public IHttpActionResult Post(List<InterviewerExceptionInfo> exceptions)
        {
            foreach (var exception in exceptions)
            {
                this.syncLogRepository.Store(new SynchronizationLogItem
                {
                    InterviewerId = exception.InterviewerId,
                    LogDate = DateTime.UtcNow,
                    Type = SynchronizationLogType.DeviceUnexpectedException,
                    Log = $@"<font color=""red"">{exception.StackTrace}</font>"
                }, Guid.NewGuid());
            }

            return this.Ok();
        }
    }
}
