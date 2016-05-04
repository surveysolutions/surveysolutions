using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.SynchronizationLog;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Synchronization;
using WB.Core.Synchronization.MetaInfo;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer
{
    public class InterviewsControllerBase : ApiController
    {
        private readonly IPlainInterviewFileStorage plainInterviewFileStorage;
        protected readonly IIncomingSyncPackagesQueue incomingSyncPackagesQueue;
        protected readonly ICommandService commandService;
        protected readonly IQueryableReadSideRepositoryReader<InterviewSyncPackageMeta> syncPackagesMetaReader;
        protected readonly IMetaInfoBuilder metaBuilder;
        protected readonly ISerializer serializer;
        protected readonly IGlobalInfoProvider globalInfoProvider;
        protected readonly IInterviewInformationFactory interviewsFactory;

        public InterviewsControllerBase(
            IPlainInterviewFileStorage plainInterviewFileStorage,
            IGlobalInfoProvider globalInfoProvider,
            IInterviewInformationFactory interviewsFactory,
            IIncomingSyncPackagesQueue incomingSyncPackagesQueue,
            ICommandService commandService,
            IQueryableReadSideRepositoryReader<InterviewSyncPackageMeta> syncPackagesMetaReader,
            IMetaInfoBuilder metaBuilder,
            ISerializer serializer)
        {
            this.plainInterviewFileStorage = plainInterviewFileStorage;
            this.globalInfoProvider = globalInfoProvider;
            this.interviewsFactory = interviewsFactory;
            this.incomingSyncPackagesQueue = incomingSyncPackagesQueue;
            this.commandService = commandService;
            this.syncPackagesMetaReader = syncPackagesMetaReader;
            this.metaBuilder = metaBuilder;
            this.serializer = serializer;
        }
        
        [WriteToSyncLog(SynchronizationLogType.GetInterviews)]
        public virtual HttpResponseMessage Get()
        {
            var resultValue = this.interviewsFactory.GetInProgressInterviews(this.globalInfoProvider.GetCurrentUser().Id)
                .Select(interview => new InterviewApiView()
                {
                    Id = interview.Id,
                    QuestionnaireIdentity = interview.QuestionnaireIdentity,
                    IsRejected = interview.IsRejected
                }).ToList();

            var response = this.Request.CreateResponse(resultValue);
            response.Headers.CacheControl = new CacheControlHeaderValue()
            {
                Public = false,
                NoCache = true
            };

            return response;
        }
        
        [WriteToSyncLog(SynchronizationLogType.InterviewProcessed)]
        public virtual void LogInterviewAsSuccessfullyHandled(Guid id)
        {
            this.commandService.Execute(new MarkInterviewAsReceivedByInterviewer(id, this.globalInfoProvider.GetCurrentUser().Id));
        }
        
        public virtual void PostImage(PostFileRequest request)
        {
            this.plainInterviewFileStorage.StoreInterviewBinaryDataAsync(request.InterviewId, request.FileName,
                Convert.FromBase64String(request.Data));
        }
    }
}