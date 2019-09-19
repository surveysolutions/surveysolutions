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
    public class AssignmentActionDenormalizer :
        IFunctionalHandler,
        IAsyncEventHandler<AssignmentCreated>,
        IAsyncEventHandler<AssignmentArchived>,
        IAsyncEventHandler<AssignmentUnarchived>,    
        IAsyncEventHandler<AssignmentDeleted>,    
        IAsyncEventHandler<AssignmentReassigned>,    
        IAsyncEventHandler<AssignmentReceivedByTablet>,    
        IAsyncEventHandler<AssignmentAudioRecordingChanged>,    
        IAsyncEventHandler<AssignmentWebModeChanged>,    
        IAsyncEventHandler<AssignmentQuantityChanged>
    {
        private readonly TenantDbContext dbContext;

        public AssignmentActionDenormalizer(TenantDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        private readonly Dictionary<Guid, Assignment> assignments = new Dictionary<Guid, Assignment>();
        private readonly Dictionary<Guid, Assignment> assignmentsFromDb = new Dictionary<Guid, Assignment>();
        private readonly List<AssignmentAction> actions = new List<AssignmentAction>();

        public Task Handle(PublishedEvent<AssignmentCreated> @event, CancellationToken cancellationToken = default)
        {
            assignments.Add(@event.EventSourceId, new Assignment()
            {
                Id = @event.Event.Id,
                PublicKey = @event.EventSourceId,
                ResponsibleId = @event.Event.ResponsibleId,
            });

            return AddRecord(@event.EventSourceId, @event.GlobalSequence, @event.Event.UserId, @event.Event.OriginDate.UtcDateTime, AssignmentExportedAction.Created, cancellationToken);
        }

        public Task Handle(PublishedEvent<AssignmentArchived> @event, CancellationToken cancellationToken = default)
        {
            return AddRecord(@event.EventSourceId, @event.GlobalSequence, @event.Event.UserId, @event.Event.OriginDate.UtcDateTime, AssignmentExportedAction.Archived, cancellationToken);
        }

        public Task Handle(PublishedEvent<AssignmentUnarchived> @event, CancellationToken cancellationToken = default)
        {
            return AddRecord(@event.EventSourceId, @event.GlobalSequence, @event.Event.UserId, @event.Event.OriginDate.UtcDateTime, AssignmentExportedAction.Unarchived, cancellationToken);
        }

        public Task Handle(PublishedEvent<AssignmentDeleted> @event, CancellationToken cancellationToken = default)
        {
            return AddRecord(@event.EventSourceId, @event.GlobalSequence, @event.Event.UserId, @event.Event.OriginDate.UtcDateTime, AssignmentExportedAction.Deleted, cancellationToken);
        }

        public Task Handle(PublishedEvent<AssignmentReassigned> @event, CancellationToken cancellationToken = default)
        {
            var assignment = GetAssignment(@event.EventSourceId);
            assignment.ResponsibleId = @event.Event.ResponsibleId;

            return AddRecord(@event.EventSourceId, @event.GlobalSequence, @event.Event.UserId, @event.Event.OriginDate.UtcDateTime, AssignmentExportedAction.Reassigned, cancellationToken);
        }

        public Task Handle(PublishedEvent<AssignmentReceivedByTablet> @event, CancellationToken cancellationToken = default)
        {
            return AddRecord(@event.EventSourceId, @event.GlobalSequence, @event.Event.UserId, @event.Event.OriginDate.UtcDateTime, AssignmentExportedAction.ReceivedByTablet, cancellationToken);
        }

        public Task Handle(PublishedEvent<AssignmentAudioRecordingChanged> @event, CancellationToken cancellationToken = default)
        {
            return AddRecord(@event.EventSourceId, @event.GlobalSequence, @event.Event.UserId, @event.Event.OriginDate.UtcDateTime, AssignmentExportedAction.AudioRecordingChanged, cancellationToken);
        }

        public Task Handle(PublishedEvent<AssignmentWebModeChanged> @event, CancellationToken cancellationToken = default)
        {
            return AddRecord(@event.EventSourceId, @event.GlobalSequence, @event.Event.UserId, @event.Event.OriginDate.UtcDateTime, AssignmentExportedAction.WebModeChanged, cancellationToken);
        }

        public Task Handle(PublishedEvent<AssignmentQuantityChanged> @event, CancellationToken cancellationToken = default)
        {
            return AddRecord(@event.EventSourceId, @event.GlobalSequence, @event.Event.UserId, @event.Event.OriginDate.UtcDateTime, AssignmentExportedAction.QuantityChanged, cancellationToken);
        }

        private Task AddRecord(Guid publicKey, long globalSequence, Guid actorId, DateTime dateTime, AssignmentExportedAction action, CancellationToken cancellationToken)
        {
            var assignment = GetAssignment(publicKey);

            var assignmentAction = new AssignmentAction()
            {
                SequenceIndex = globalSequence,
                AssignmentId = assignment.Id,
                Timestamp = dateTime,
                Status = action,
                OriginatorId = actorId,
                ResponsibleId = assignment.ResponsibleId,
            };
            actions.Add(assignmentAction);
            return Task.CompletedTask;
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
