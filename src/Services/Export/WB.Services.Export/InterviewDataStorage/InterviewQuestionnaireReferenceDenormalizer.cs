using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly Dictionary<Guid, string> added = new Dictionary<Guid, string>();
        private readonly HashSet<Guid> removed = new HashSet<Guid>();

        public void Handle(PublishedEvent<InterviewCreated> @event)
        {
            added.Add(@event.EventSourceId, @event.Event.QuestionnaireIdentity);
        }

        public void Handle(PublishedEvent<InterviewHardDeleted> @event)
        {
            if (!added.Remove(@event.EventSourceId))
            {
                removed.Add(@event.EventSourceId);
            }
        }

        public async Task SaveStateAsync(CancellationToken cancellationToken = default)
        {
            var db = this.tenantContext.DbContext;

            db.InterviewReferences.AddRange(added.Select(kv => new InterviewQuestionnaireReferenceNode
            {
                InterviewId = kv.Key,
                QuestionnaireId = kv.Value
            }));

            var arg = new object[1];
            foreach (var remove in removed)
            {
                arg[0] = remove;
                var reference = await db.InterviewReferences.FindAsync(arg, cancellationToken);
                db.InterviewReferences.Remove(reference);
            }

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
