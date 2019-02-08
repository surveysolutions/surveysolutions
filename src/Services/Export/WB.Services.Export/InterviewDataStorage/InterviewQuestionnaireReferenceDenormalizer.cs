using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Events.Interview;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.InterviewDataStorage
{
    public class InterviewQuestionnaireReferenceDenormalizer:
        IHighPriorityFunctionalHandler,
        IEventHandler<InterviewCreated>,
        IEventHandler<InterviewHardDeleted>
    {
        private readonly ITenantContext tenantContext;
        private readonly IInterviewQuestionnaireReferenceStorage referenceStorage;

        public InterviewQuestionnaireReferenceDenormalizer(ITenantContext tenantContext)
        {
            this.tenantContext = tenantContext;
        }

        public Task HandleAsync(PublishedEvent<InterviewCreated> @event, CancellationToken cancellationToken = default)
        {
            return this.tenantContext.DbContext.InterviewReferences.AddAsync(
                new InterviewQuestionnaireReferenceNode
                {
                    InterviewId = @event.EventSourceId,
                    QuestionnaireId = @event.Event.QuestionnaireId.ToString("N") + "$" +
                                      @event.Event.QuestionnaireVersion
                }, cancellationToken);
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
