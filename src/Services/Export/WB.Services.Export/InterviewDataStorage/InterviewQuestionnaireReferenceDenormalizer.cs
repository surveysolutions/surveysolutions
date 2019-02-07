using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Events.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.InterviewDataStorage
{
    public class InterviewQuestionnaireReferenceDenormalizer:
        IFunctionalHandler,
        IEventHandler<InterviewCreated>,
        IEventHandler<InterviewHardDeleted>
    {
        private readonly IInterviewQuestionnaireReferenceStorage referenceStorage;

        public InterviewQuestionnaireReferenceDenormalizer(IInterviewQuestionnaireReferenceStorage referenceStorage)
        {
            this.referenceStorage = referenceStorage;
        }

        public Task HandleAsync(PublishedEvent<InterviewCreated> @event, CancellationToken cancellationToken = default)
        {
            string questionnaireId = $"{@event.Event.QuestionnaireId}${@event.Event.QuestionnaireVersion}";
            referenceStorage.AddInterviewQuestionnaireReference(@event.EventSourceId, new QuestionnaireId(questionnaireId));
            return Task.CompletedTask;
        }

        public Task HandleAsync(PublishedEvent<InterviewHardDeleted> @event, CancellationToken cancellationToken = default)
        {
            referenceStorage.RemoveInterviewQuestionnaireReference(@event.EventSourceId);
            return Task.CompletedTask;
        }

        public Task SaveStateAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
