using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.SynchronizationLog;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Synchronization;
using WB.Core.Synchronization.MetaInfo;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v2
{
    [ApiBasicAuth(new[] { UserRoles.Operator })]
    public class InterviewsApiV2Controller : InterviewsControllerBase
    {
        private readonly IStringCompressor compressor;

        public InterviewsApiV2Controller(
            IPlainInterviewFileStorage plainInterviewFileStorage,
            IGlobalInfoProvider globalInfoProvider,
            IInterviewInformationFactory interviewsFactory,
            IIncomingSyncPackagesQueue incomingSyncPackagesQueue,
            ICommandService commandService,
            IQueryableReadSideRepositoryReader<InterviewSyncPackageMeta> syncPackagesMetaReader,
            IMetaInfoBuilder metaBuilder,
            ISerializer serializer,
            IStringCompressor compressor) : base(
                plainInterviewFileStorage: plainInterviewFileStorage,
                globalInfoProvider: globalInfoProvider,
                interviewsFactory: interviewsFactory,
                incomingSyncPackagesQueue: incomingSyncPackagesQueue,
                commandService: commandService,
                syncPackagesMetaReader: syncPackagesMetaReader,
                metaBuilder: metaBuilder,
                serializer: serializer)
        {
            this.compressor = compressor;
        }

        [HttpGet]
        public override HttpResponseMessage Get() => base.Get();

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetInterview)]
        public HttpResponseMessage Details(Guid id)
        {
            var interviewDetails = this.interviewsFactory.GetInterviewDetails(id);
            if (interviewDetails == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var interviewMetaInfo = this.metaBuilder.GetInterviewMetaInfo(interviewDetails);

            var response = this.Request.CreateResponse(new InterviewerInterviewApiView
            {
                AnswersOnPrefilledQuestions = interviewMetaInfo?.FeaturedQuestionsMeta
                    .Select(prefilledQuestion => new AnsweredQuestionSynchronizationDto(prefilledQuestion.PublicKey, new decimal[0], prefilledQuestion.Value, string.Empty))
                    .ToArray(),
                Details = interviewDetails
            });

            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                Public = false,
                NoCache = true
            };

            return response;
        }
        [HttpPost]
        public override void LogInterviewAsSuccessfullyHandled(Guid id) => base.LogInterviewAsSuccessfullyHandled(id);

        [HttpPost]
        public void Post(InterviewPackageApiView package)
        {
            this.incomingSyncPackagesQueue.Enqueue(
                interviewId: package.InterviewId,
                item: this.serializer.Serialize(new SyncItem
                {
                    RootId = package.InterviewId,
                    MetaInfo = this.compressor.CompressString(this.serializer.Serialize(package.MetaInfo)),
                    Content = this.compressor.CompressString(this.serializer.Serialize(package.Events)),
                    IsCompressed = true,
                    ItemType = SyncItemType.Interview
                }));
        }
        [HttpPost]
        public override void PostImage(PostFileRequest request) => base.PostImage(request);
    }
}