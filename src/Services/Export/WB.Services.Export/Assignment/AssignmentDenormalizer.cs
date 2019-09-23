using System;
using System.Collections.Generic;
using System.Globalization;
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
                Quantity = @event.Event.Quantity,
                WebMode = @event.Event.WebMode,
                AudioRecording = @event.Event.AudioRecording,
                Comment = @event.Event.Comment,
            });

            AddRecord(@event.EventSourceId, @event.GlobalSequence, @event.Event.UserId, @event.Event.OriginDate, AssignmentExportedAction.Created, null, null, @event.Event.Comment);
            AddRecord(@event.EventSourceId, @event.GlobalSequence, @event.Event.UserId, @event.Event.OriginDate, AssignmentExportedAction.QuantityChanged, null, ToQuantityString(@event.Event.Quantity), null);
            AddRecord(@event.EventSourceId, @event.GlobalSequence, @event.Event.UserId, @event.Event.OriginDate, AssignmentExportedAction.WebModeChanged, null, ToWebModeString(@event.Event.WebMode), null);
            AddRecord(@event.EventSourceId, @event.GlobalSequence, @event.Event.UserId, @event.Event.OriginDate, AssignmentExportedAction.AudioRecordingChanged, null, ToAudioRecordingString(@event.Event.AudioRecording), null);
        }

        public void Handle(PublishedEvent<AssignmentArchived> @event)
        {
            AddRecord(@event.EventSourceId, @event.GlobalSequence, @event.Event.UserId, @event.Event.OriginDate, AssignmentExportedAction.Archived, null, null, null);
        }

        public void Handle(PublishedEvent<AssignmentUnarchived> @event)
        {
            AddRecord(@event.EventSourceId, @event.GlobalSequence, @event.Event.UserId, @event.Event.OriginDate, AssignmentExportedAction.Unarchived, null, null, null);
        }

        public void Handle(PublishedEvent<AssignmentDeleted> @event)
        {
            AddRecord(@event.EventSourceId, @event.GlobalSequence, @event.Event.UserId, @event.Event.OriginDate, AssignmentExportedAction.Deleted, null, null, null);
        }

        public void Handle(PublishedEvent<AssignmentReassigned> @event)
        {
            var assignment = GetAssignment(@event.EventSourceId);
            assignment.ResponsibleId = @event.Event.ResponsibleId;
            assignment.Comment = @event.Event.Comment;

            AddRecord(@event.EventSourceId, @event.GlobalSequence, @event.Event.UserId, @event.Event.OriginDate, AssignmentExportedAction.Reassigned, null, null, @event.Event.Comment);
        }

        public void Handle(PublishedEvent<AssignmentReceivedByTablet> @event)
        {
            AddRecord(@event.EventSourceId, @event.GlobalSequence, @event.Event.UserId, @event.Event.OriginDate, AssignmentExportedAction.ReceivedByTablet, null, null, null);
        }

        public void Handle(PublishedEvent<AssignmentAudioRecordingChanged> @event)
        {
            var assignment = GetAssignment(@event.EventSourceId);

            AddRecord(@event.EventSourceId, @event.GlobalSequence, @event.Event.UserId, @event.Event.OriginDate, AssignmentExportedAction.AudioRecordingChanged, 
                ToAudioRecordingString(assignment.AudioRecording), ToAudioRecordingString(@event.Event.AudioRecording), null);

            assignment.AudioRecording = @event.Event.AudioRecording;
        }

        public void Handle(PublishedEvent<AssignmentWebModeChanged> @event)
        {
            var assignment = GetAssignment(@event.EventSourceId);

            AddRecord(@event.EventSourceId, @event.GlobalSequence, @event.Event.UserId, @event.Event.OriginDate, AssignmentExportedAction.WebModeChanged,
                ToWebModeString(assignment.WebMode), ToWebModeString(@event.Event.WebMode), null);

            assignment.WebMode = @event.Event.WebMode;
        }

        public void Handle(PublishedEvent<AssignmentQuantityChanged> @event)
        {
            var assignment = GetAssignment(@event.EventSourceId);

            AddRecord(@event.EventSourceId, @event.GlobalSequence, @event.Event.UserId, @event.Event.OriginDate, AssignmentExportedAction.QuantityChanged,
                ToQuantityString(assignment.Quantity), ToQuantityString(@event.Event.Quantity), null);

            assignment.Quantity = @event.Event.Quantity;
        }

        private void AddRecord(Guid publicKey, long globalSequence, Guid actorId, DateTimeOffset dateTimeOffset, AssignmentExportedAction action, 
            string oldValue, string newValue, string comment)
        {
            var assignment = GetAssignment(publicKey);

            var assignmentAction = new AssignmentAction()
            {
                Sequence = globalSequence,
                AssignmentId = assignment.Id,
                TimestampUtc = dateTimeOffset.UtcDateTime,
                Status = action,
                OriginatorId = actorId,
                ResponsibleId = assignment.ResponsibleId,
                OldValue = oldValue,
                NewValue = newValue,
                Comment = comment,
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

        private string ToQuantityString(int? quantity) => quantity.HasValue ? quantity.Value.ToString(CultureInfo.InvariantCulture) : "-1";
        private string ToWebModeString(bool? webMode) => webMode == false ? "0" : "1";
        private string ToAudioRecordingString(bool audioRecording) => audioRecording == false ? "0" : "1";
    }
}
