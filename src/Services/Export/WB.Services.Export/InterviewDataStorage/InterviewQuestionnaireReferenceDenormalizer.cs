using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Events.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.InterviewDataStorage
{
    public class InterviewQuestionnaireReferenceDenormalizer:
        IHighPriorityFunctionalHandler,
        IEventHandler<InterviewCreated>,
        IEventHandler<InterviewHardDeleted>
    {
        private readonly IInterviewQuestionnaireReferenceStorage referenceStorage;

        public InterviewQuestionnaireReferenceDenormalizer(IInterviewQuestionnaireReferenceStorage referenceStorage)
        {
            this.referenceStorage = referenceStorage;
        }

        public async Task HandleAsync(PublishedEvent<InterviewCreated> @event, CancellationToken cancellationToken = default)
        {
            string questionnaireId = $"{@event.Event.QuestionnaireId}${@event.Event.QuestionnaireVersion}";
            var id = new QuestionnaireId(questionnaireId);
            await referenceStorage.AddInterviewQuestionnaireReferenceAsync(@event.EventSourceId, id, cancellationToken);
        }

        public Task HandleAsync(PublishedEvent<InterviewHardDeleted> @event, CancellationToken cancellationToken = default)
        {
            return referenceStorage.RemoveInterviewQuestionnaireReferenceAsync(@event.EventSourceId, cancellationToken);
        }

        public Task SaveStateAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
