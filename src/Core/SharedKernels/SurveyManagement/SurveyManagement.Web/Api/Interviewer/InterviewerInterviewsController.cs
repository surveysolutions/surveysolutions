using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Synchronization;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer
{
    [ApiBasicAuth]
    [RoutePrefix("api/interviewer/v1/interviews")]
    [ProtobufJsonSerializer]
    public class InterviewerInterviewsController : ApiController
    {
        private readonly IPlainInterviewFileStorage plainInterviewFileStorage;
        private readonly ISyncManager syncManager;
        private readonly IGlobalInfoProvider globalInfoProvider;
        private readonly IUserWebViewFactory userInfoViewFactory;
        private readonly IInterviewerInterviewsFactory interviewerInterviewsFactory;

        public InterviewerInterviewsController(
            IPlainInterviewFileStorage plainInterviewFileStorage,
            ISyncManager syncManager,
            IGlobalInfoProvider globalInfoProvider,
            IUserWebViewFactory userInfoViewFactory,
            IInterviewerInterviewsFactory interviewerInterviewsFactory)
        {
            this.plainInterviewFileStorage = plainInterviewFileStorage;
            this.syncManager = syncManager;
            this.globalInfoProvider = globalInfoProvider;
            this.userInfoViewFactory = userInfoViewFactory;
            this.interviewerInterviewsFactory = interviewerInterviewsFactory;
        }

        [HttpGet]
        [Route("")]
        public List<InterviewApiView> Get()
        {
            return this.interviewerInterviewsFactory.GetInProgressInterviews(this.globalInfoProvider.GetCurrentUser().Id).Select(interview => new InterviewApiView()
            {
                Id = interview.Id,
                QuestionnaireIdentity = interview.QuestionnaireIdentity
            }).ToList();
        }

        [HttpGet]
        [Route("packages/{lastPackageId?}")]
        public List<SynchronizationChunkMeta> GetPackages(string lastPackageId = null)
        {
            return this.syncManager.GetInterviewPackageIdsWithOrder(
                userId: this.globalInfoProvider.GetCurrentUser().Id,
                deviceId: this.GetInterviewerDeviceId(),
                lastSyncedPackageId: lastPackageId).SyncPackagesMeta.ToList();
        }

        [HttpGet]
        [Route("package/{id}/{previousSuccessfullyHandledPackageId?}")]
        public InterviewSyncPackageDto GetPackage(string id, string previousSuccessfullyHandledPackageId = null)
        {
            if (!string.IsNullOrEmpty(previousSuccessfullyHandledPackageId))
                this.syncManager.MarkPackageAsSuccessfullyHandled(previousSuccessfullyHandledPackageId, this.GetInterviewerDeviceId(), this.globalInfoProvider.GetCurrentUser().Id);

            return this.syncManager.ReceiveInterviewSyncPackage(
                userId: this.globalInfoProvider.GetCurrentUser().Id,
                deviceId: this.GetInterviewerDeviceId(), 
                packageId: id);
        }

        [HttpPost]
        [Route("package/{id:guid}")]
        public HttpResponseMessage Post(Guid id, [FromBody]string package)
        {
            this.syncManager.SendSyncItem(interviewId: id, package: package);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("{id:guid}/image")]
        public HttpResponseMessage PostImage(PostFileRequest request)
        {
            this.plainInterviewFileStorage.StoreInterviewBinaryData(request.InterviewId, request.FileName, Convert.FromBase64String(request.Data));
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("package/{id}/logstate")]
        public void LogPackageAsSuccessfullyHandled(string id)
        {
            if (!string.IsNullOrEmpty(id))
                this.syncManager.MarkPackageAsSuccessfullyHandled(id, this.GetInterviewerDeviceId(), this.globalInfoProvider.GetCurrentUser().Id);
        }

        private Guid GetInterviewerDeviceId()
        {
            return this.userInfoViewFactory.Load(new UserWebViewInputModel(this.globalInfoProvider.GetCurrentUser().Name, null)).DeviceId.ToGuid();
        }
    }
}