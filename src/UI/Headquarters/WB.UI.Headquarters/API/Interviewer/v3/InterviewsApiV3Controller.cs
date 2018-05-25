using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v2;
using WB.Core.Synchronization.MetaInfo;
using WB.Infrastructure.Native.Storage;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.Interviewer.v3
{
    [ApiBasicAuth(new[] { UserRoles.Interviewer })]
    public class InterviewsApiV3Controller : InterviewsApiV2Controller
    {
        private readonly IEventStore eventStore;

        public InterviewsApiV3Controller(IImageFileStorage imageFileStorage,
            IAudioFileStorage audioFileStorage, 
            IAuthorizedUser authorizedUser, 
            IInterviewInformationFactory interviewsFactory, 
            IInterviewPackagesService incomingSyncPackagesQueue, 
            ICommandService commandService, 
            IEventStore eventStore,
            IMetaInfoBuilder metaBuilder, 
            IJsonAllTypesSerializer synchronizationSerializer) : 
                base(imageFileStorage, audioFileStorage, authorizedUser, interviewsFactory, incomingSyncPackagesQueue, commandService, metaBuilder, synchronizationSerializer)
        {
            this.eventStore = eventStore;
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetInterview)]
        public JsonResult<List<CommittedEvent>> DetailsV3(Guid id)
        {
            var allEvents = eventStore.Read(id, 0).ToList();
            return Json(allEvents, EventSerializerSettings.SyncronizationJsonSerializerSettings);
        }
    }
}
