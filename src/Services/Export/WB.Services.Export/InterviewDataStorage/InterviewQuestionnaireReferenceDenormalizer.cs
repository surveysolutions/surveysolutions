using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Events.Interview;
using WB.Services.Export.Infrastructure;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.InterviewDataStorage
{
    public class InterviewQuestionnaireReferenceDenormalizer:
        IHighPriorityFunctionalHandler,
        IEventHandler<InterviewCreated>,
        IEventHandler<InterviewHardDeleted>
    {
        private readonly ITenantContext tenantContext;

        public InterviewQuestionnaireReferenceDenormalizer(ITenantContext tenantContext)
        {
            this.tenantContext = tenantContext;
        }

        public void Handle(PublishedEvent<InterviewCreated> @event)
        {
            this.tenantContext.DbContext.InterviewReferences.Add(
                new InterviewQuestionnaireReferenceNode
                {
                    InterviewId = @event.EventSourceId,
                    QuestionnaireId = @event.Event.QuestionnaireId.ToString("N") + "$" +
                                      @event.Event.QuestionnaireVersion
                });
        }

        public void Handle(PublishedEvent<InterviewHardDeleted> @event)
        {
            var dbContext = this.tenantContext.DbContext;
            var reference = dbContext.InterviewReferences.Find(@event.EventSourceId);
            dbContext.InterviewReferences.Remove(reference);
        }

        public Task SaveStateAsync(CancellationToken cancellationToken = default)
        {
            return this.tenantContext.DbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
