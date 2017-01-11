using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Services.WebInterview;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    internal class InterviewValidationHandlerFunctional :
        AbstractFunctionalEventHandler<InterviewData, IReadSideKeyValueStorage<InterviewData>>,
        IUpdateHandler<InterviewData, AnswersDeclaredInvalid>, IUpdateHandler<InterviewData, AnswersDeclaredValid>
    {
        private readonly IWebInterviewExchangeService webInterviewExchangeService;

        public InterviewValidationHandlerFunctional(IReadSideKeyValueStorage<InterviewData> readSideStorage, IWebInterviewExchangeService webInterviewExchangeService)
            : base(readSideStorage)
        {
            this.webInterviewExchangeService = webInterviewExchangeService;
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<AnswersDeclaredInvalid> @event)
        {
            this.webInterviewExchangeService.AnswersDeclaredInvalid(state.InterviewId, @event.Payload.Questions);
            return state;
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<AnswersDeclaredValid> @event)
        {
            this.webInterviewExchangeService.AnswersDeclaredValid(state.InterviewId, @event.Payload.Questions);
            return state;
        }
    }
}