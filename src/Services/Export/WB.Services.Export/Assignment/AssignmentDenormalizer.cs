using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Events.Assignment;
using WB.Services.Export.Infrastructure;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.Assignment
{
    public class AssignmentDenormalizer :
        IFunctionalHandler,
        IEventHandler<AssignmentCreated>,
        IEventHandler<AssignmentArchived>,
        IEventHandler<AssignmentUnarchived>,    
        IEventHandler<AssignmentDeleted>,    
        IEventHandler<AssignmentReassigned>,    
        IEventHandler<AssignmentReceivedByTablet>,    
        IEventHandler<AssignmentAudioRecordingChanged>,    
        IEventHandler<AssignmentWebModeChanged>,    
        IEventHandler<AssignmentQuantityChanged>
    {
        private readonly TenantDbContext dbContext;

        public AssignmentDenormalizer(TenantDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        private readonly Dictionary<Guid, Assignment> assignments = new Dictionary<Guid, Assignment>();
        private readonly Dictionary<Guid, Assignment> assignmentsFromDb = new Dictionary<Guid, Assignment>();
        private readonly List<AssignmentAction> actions = new List<AssignmentAction>();

        public void Handle(PublishedEvent<AssignmentCreated> @event)
        {
            assignments.Add(@event.EventSourceId, new Assignment()
            {
                Id = @event.Event.Id,
                PublicKey = @event.EventSourceId,
                ResponsibleId = @event.Event.ResponsibleId,
            });

            AddRecord(@event.EventSourceId, @event.GlobalSequence, @event.Event.UserId, @event.Event.OriginDate, AssignmentExportedAction.Created);
        }

        public void Handle(PublishedEvent<AssignmentArchived> @event)
        {
            AddRecord(@event.EventSourceId, @event.GlobalSequence, @event.Event.UserId, @event.Event.OriginDate, AssignmentExportedAction.Archived);
        }

        public void Handle(PublishedEvent<AssignmentUnarchived> @event)
        {
            AddRecord(@event.EventSourceId, @event.GlobalSequence, @event.Event.UserId, @event.Event.OriginDate, AssignmentExportedAction.Unarchived);
        }

        public void Handle(PublishedEvent<AssignmentDeleted> @event)
        {
            AddRecord(@event.EventSourceId, @event.GlobalSequence, @event.Event.UserId, @event.Event.OriginDate, AssignmentExportedAction.Deleted);
        }

        public void Handle(PublishedEvent<AssignmentReassigned> @event)
        {
            var assignment = GetAssignment(@event.EventSourceId);
            assignment.ResponsibleId = @event.Event.ResponsibleId;

            AddRecord(@event.EventSourceId, @event.GlobalSequence, @event.Event.UserId, @event.Event.OriginDate, AssignmentExportedAction.Reassigned);
        }

        public void Handle(PublishedEvent<AssignmentReceivedByTablet> @event)
        {
            AddRecord(@event.EventSourceId, @event.GlobalSequence, @event.Event.UserId, @event.Event.OriginDate, AssignmentExportedAction.ReceivedByTablet);
        }

        public void Handle(PublishedEvent<AssignmentAudioRecordingChanged> @event)
        {
            AddRecord(@event.EventSourceId, @event.GlobalSequence, @event.Event.UserId, @event.Event.OriginDate, AssignmentExportedAction.AudioRecordingChanged);
        }

        public void Handle(PublishedEvent<AssignmentWebModeChanged> @event)
        {
            AddRecord(@event.EventSourceId, @event.GlobalSequence, @event.Event.UserId, @event.Event.OriginDate, AssignmentExportedAction.WebModeChanged);
        }

        public void Handle(PublishedEvent<AssignmentQuantityChanged> @event)
        {
            AddRecord(@event.EventSourceId, @event.GlobalSequence, @event.Event.UserId, @event.Event.OriginDate, AssignmentExportedAction.QuantityChanged);
        }

        private void AddRecord(Guid publicKey, long globalSequence, Guid actorId, DateTimeOffset dateTimeOffset, AssignmentExportedAction action)
        {
            var assignment = GetAssignment(publicKey);

            var assignmentAction = new AssignmentAction()
            {
                SequenceIndex = globalSequence,
                AssignmentId = assignment.Id,
                TimestampUtc = dateTimeOffset.UtcDateTime,
                Status = action,
                OriginatorId = actorId,
                ResponsibleId = assignment.ResponsibleId,
            };
            actions.Add(assignmentAction);
        }

        private Assignment GetAssignment(Guid publicKey)
        {
            if (assignments.TryGetValue(publicKey, out Assignment assignment))
                return assignment;

            if (assignmentsFromDb.TryGetValue(publicKey, out Assignment assignmentFromDbCached))
                return assignmentFromDbCached;

            //var assignmentFromDb = await dbContext.Assignments.FindAsync(publicKey, cancellationToken);
            var assignmentFromDb = dbContext.Assignments.Where(ass => ass.PublicKey == publicKey).First();
            assignmentsFromDb.Add(publicKey, assignmentFromDb);
            return assignmentFromDb;
        }

        public async Task SaveStateAsync(CancellationToken cancellationToken = default)
        {
            foreach (var assignment in assignments)
            {
                await dbContext.Assignments.AddAsync(assignment.Value, cancellationToken);
            }

            foreach (var assignment in assignmentsFromDb)
            {
                dbContext.Assignments.Update(assignment.Value);
            }

            foreach (var assignmentAction in actions)
            {
                await dbContext.AssignmentActions.AddAsync(assignmentAction, cancellationToken);
            }
        }
    }
}
