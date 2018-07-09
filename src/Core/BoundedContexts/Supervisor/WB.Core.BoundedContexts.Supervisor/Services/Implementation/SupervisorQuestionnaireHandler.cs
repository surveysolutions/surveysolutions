using System.Threading.Tasks;
using Ncqrs.Eventing;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation
{
    public class SupervisorQuestionnaireHandler
        :   IHandleCommunicationMessage
    {
        private readonly ILiteEventBus eventBus;
        private readonly IEnumeratorEventStorage eventStore;
        

        public SupervisorQuestionnaireHandler(ILiteEventBus eventBus,
            IEnumeratorEventStorage eventStorage)
        {
            this.eventBus = eventBus;
            this.eventStore = eventStorage;
        }

        public Task<GetQuestionnaireListResponse> Handle(GetQuestionnaireListRequest message)
        {
            var result = new GetQuestionnaireListResponse
            {
                Questionnaires = {"questionnaireId#1"}
            };

            return Task.FromResult(result);
        }

        public Task<SendBigAmountOfDataResponse> Handle(SendBigAmountOfDataRequest request)
        {
            return Task.FromResult(new SendBigAmountOfDataResponse
            {
                Data = request.Data
            });
        }

        public void Register(IRequestHandler requestHandler)
        {
            requestHandler.RegisterHandler<PostInterviewRequest, OkResponse>(Handle);
            requestHandler.RegisterHandler<GetQuestionnaireListRequest, GetQuestionnaireListResponse>(Handle);
            requestHandler.RegisterHandler<SendBigAmountOfDataRequest, SendBigAmountOfDataResponse>(Handle);
            requestHandler.RegisterHandler<CanSynchronizeRequest, CanSynchronizeResponse>(Handle);
        }

        public Task<CanSynchronizeResponse> Handle(CanSynchronizeRequest arg)
        {
            var expectedVersion = ReflectionUtils.GetAssemblyVersion(typeof(SupervisorBoundedContextAssemblyIndicator));

            return Task.FromResult(new CanSynchronizeResponse
            {
                CanSyncronize = expectedVersion.Revision == arg.InterviewerBuildNumber
            });
        }

        private Task<OkResponse> Handle(PostInterviewRequest arg)
        {
            eventBus.PublishCommittedEvents(arg.Events);
            eventStore.StoreEvents(new CommittedEventStream(arg.InterviewId, arg.Events));

            return Task.FromResult(new OkResponse());
        }
    }
}
