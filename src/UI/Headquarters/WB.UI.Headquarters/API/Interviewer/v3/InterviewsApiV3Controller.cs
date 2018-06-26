using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Code;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.API.Interviewer.v3
{
    [ApiBasicAuth(new[] { UserRoles.Interviewer })]
    public class InterviewsApiV3Controller : ApiController
    {
        private readonly IHeadquartersEventStore eventStore;
        private readonly IInterviewPackagesService packagesService;

        public InterviewsApiV3Controller(
            IHeadquartersEventStore eventStore,
            IInterviewPackagesService packagesService)
        {
            this.eventStore = eventStore;
            this.packagesService = packagesService;
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetInterviewV3)]
        public JsonResult<List<CommittedEvent>> DetailsV3(Guid id)
        {
            var allEvents = eventStore.Read(id, 0).ToList();
            return Json(allEvents, Infrastructure.Native.Storage.EventSerializerSettings.SyncronizationJsonSerializerSettings);
        }

        [WriteToSyncLog(SynchronizationLogType.PostInterviewV3)]
        [HttpPost]
        public HttpResponseMessage PostV3(InterviewPackageApiView package)
        {
            if (string.IsNullOrEmpty(package.Events))
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Server cannot accept empty package content.");

            var interviewPackage = new InterviewPackage
            {
                InterviewId = package.InterviewId,
                QuestionnaireId = package.MetaInfo.TemplateId,
                QuestionnaireVersion = package.MetaInfo.TemplateVersion,
                InterviewStatus = (InterviewStatus)package.MetaInfo.Status,
                ResponsibleId = package.MetaInfo.ResponsibleId,
                IsCensusInterview = package.MetaInfo.CreatedOnClient ?? false,
                IncomingDate = DateTime.UtcNow,
                Events = package.Events
            };

            this.packagesService.StoreOrProcessPackage(interviewPackage);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        [WriteToSyncLog(SynchronizationLogType.CheckObsoleteInterviews)]
        public HttpResponseMessage CheckObsoleteInterviews(List<ObsoletePackageCheck> knownPackages)
        {
            List<Guid> obsoleteInterviews = new List<Guid>();
            foreach (var obsoletePackageCheck in knownPackages)
            {
                if (this.eventStore.HasEventsAfterSpecifiedSequenceWithAnyOfSpecifiedTypes(
                    obsoletePackageCheck.SequenceOfLastReceivedEvent,
                    obsoletePackageCheck.InterviewId, EventsThatChangeAnswersStateProvider.GetTypeNames()))
                {
                    obsoleteInterviews.Add(obsoletePackageCheck.InterviewId);
                } 
            }

            return Request.CreateResponse(HttpStatusCode.OK, obsoleteInterviews);
        } 
    }
}
