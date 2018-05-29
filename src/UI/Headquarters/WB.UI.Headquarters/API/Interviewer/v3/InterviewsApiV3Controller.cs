using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.Interviewer.v3
{
    [ApiBasicAuth(new[] { UserRoles.Interviewer })]
    public class InterviewsApiV3Controller : ApiController
    {
        private readonly IEventStore eventStore;

        public InterviewsApiV3Controller(
            IEventStore eventStore) 
        {
            this.eventStore = eventStore;
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetInterview)]
        public JsonResult<List<CommittedEvent>> DetailsV3(Guid id)
        {
            var allEvents = eventStore.Read(id, 0).ToList();
            return Json(allEvents, WB.Infrastructure.Native.Storage.EventSerializerSettings.SyncronizationJsonSerializerSettings);
        }
    }
}
