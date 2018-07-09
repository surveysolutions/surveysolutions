using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation
{
    public class SupervisorQuestionnaireHandler
        :   IHandleCommunicationMessage
    {
        private readonly ILiteEventBus eventBus;
        private readonly IEnumeratorEventStorage eventStore;
        private readonly IQuestionnaireAssemblyAccessor questionnaireAssemblyAccessor;
        private readonly IInterviewerQuestionnaireAccessor questionnaireAccessor;
        private readonly ISerializer serializer;
        private readonly IPlainStorage<InterviewView> interviews;

        public SupervisorQuestionnaireHandler(ILiteEventBus eventBus,
            IEnumeratorEventStorage eventStorage,
            IQuestionnaireAssemblyAccessor questionnaireAssemblyAccessor, 
            ISerializer serializer, 
            IInterviewerQuestionnaireAccessor questionnaireAccessor, 
            IPlainStorage<InterviewView> interviews)
        {
            this.eventBus = eventBus;
            this.eventStore = eventStorage;
            this.questionnaireAssemblyAccessor = questionnaireAssemblyAccessor;
            this.serializer = serializer;
            this.questionnaireAccessor = questionnaireAccessor;
            this.interviews = interviews;
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
            requestHandler.RegisterHandler<GetQuestionnaireAssemblyRequest, GetQuestionnaireAssemblyResponse>(Handle);
            requestHandler.RegisterHandler<GetQuestionnaireRequest, GetQuestionnaireResponse>(Handle);
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

        public Task<GetQuestionnaireResponse> Handle(GetQuestionnaireRequest arg)
        {
            var questionnaireDocument = this.questionnaireAccessor.GetQuestionnaire(arg.QuestionnaireId);
            var serializedDocument = this.serializer.Serialize(questionnaireDocument);
            return Task.FromResult(new GetQuestionnaireResponse
            {
                QuestionnaireDocument = serializedDocument
            });
        }

        private Task<GetQuestionnaireAssemblyResponse> Handle(GetQuestionnaireAssemblyRequest arg)
        {
            var assembly =
                this.questionnaireAssemblyAccessor.GetAssemblyAsByteArray(arg.QuestionnaireId.QuestionnaireId,
                    arg.QuestionnaireId.Version);

            return Task.FromResult(new GetQuestionnaireAssemblyResponse
            {
                Content = assembly
            });
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

        private Task<OkResponse> Handle(PostInterviewRequest arg)
        {
            eventBus.PublishCommittedEvents(arg.Events);
            eventStore.StoreEvents(new CommittedEventStream(arg.InterviewId, arg.Events));

            return Task.FromResult(new OkResponse());
        }
    }
}
