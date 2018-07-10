using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers
{
    public class SupervisorInterviewsHandler : IHandleCommunicationMessage
    {
        private readonly ILiteEventBus eventBus;
        private readonly IEnumeratorEventStorage eventStore;
        private readonly IPlainStorage<InterviewView> interviews;

        public SupervisorInterviewsHandler(ILiteEventBus eventBus,
            IEnumeratorEventStorage eventStore,
            IPlainStorage<InterviewView> interviews)
        {
            this.eventBus = eventBus;
            this.eventStore = eventStore;
            this.interviews = interviews;
        }

        public void Register(IRequestHandler requestHandler)
        {
            requestHandler.RegisterHandler<CanSynchronizeRequest, CanSynchronizeResponse>(Handle);
            requestHandler.RegisterHandler<PostInterviewRequest, OkResponse>(Handle);
            requestHandler.RegisterHandler<GetInterviewsRequest, GetInterviewsResponse>(Handle);
            requestHandler.RegisterHandler<LogInterviewAsSuccessfullyHandledRequest, OkResponse>(Handle);
            requestHandler.RegisterHandler<GetInterviewDetailsRequest, GetInterviewDetailsResponse>(Handle);
        }

        public Task<GetInterviewDetailsResponse> Handle(GetInterviewDetailsRequest arg)
        {
            var events = this.eventStore.Read(arg.InterviewId, 0).ToList();

            return Task.FromResult(new GetInterviewDetailsResponse
            {
                Events = events
            });
        }

        public Task<OkResponse> Handle(LogInterviewAsSuccessfullyHandledRequest arg)
        {
            this.interviews.Remove(arg.InterviewId.FormatGuid());
            return Task.FromResult(new OkResponse());
        }

        public Task<CanSynchronizeResponse> Handle(CanSynchronizeRequest arg)
        {
            var expectedVersion = ReflectionUtils.GetAssemblyVersion(typeof(SupervisorBoundedContextAssemblyIndicator));

            return Task.FromResult(new CanSynchronizeResponse
            {
                CanSyncronize = expectedVersion.Revision == arg.InterviewerBuildNumber
            });
        }

        public Task<GetInterviewsResponse> Handle(GetInterviewsRequest arg)
        {
            var interviewsForUser = this.interviews.Where(x =>
                (x.Status == InterviewStatus.RejectedBySupervisor || x.Status == InterviewStatus.InterviewerAssigned)
                && x.ResponsibleId == arg.UserId);

            List<InterviewApiView> response = interviewsForUser.Select(x => new InterviewApiView
            {
                Id = x.InterviewId,
                IsRejected = x.Status == InterviewStatus.RejectedBySupervisor,
                QuestionnaireIdentity = QuestionnaireIdentity.Parse(x.QuestionnaireId)
            }).ToList();

            return Task.FromResult(new GetInterviewsResponse
            {
                Interviews = response
            });
        }

        public Task<OkResponse> Handle(PostInterviewRequest arg)
        {
            eventBus.PublishCommittedEvents(arg.Events);
            eventStore.StoreEvents(new CommittedEventStream(arg.InterviewId, arg.Events));

            return Task.FromResult(new OkResponse());
        }
    }
}
