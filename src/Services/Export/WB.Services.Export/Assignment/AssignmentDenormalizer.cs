using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Events.Assignment;
using WB.Services.Export.Events.Assignment.Base;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Questionnaire;
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


        public void Handle(PublishedEvent<AssignmentCreated> @event)
        {
            dbContext.Assignments.Add(new Assignment
            {
                Id = @event.Event.Id,
                PublicKey = @event.EventSourceId,
                ResponsibleId = @event.Event.ResponsibleId,
                Quantity = @event.Event.Quantity,
                WebMode = @event.Event.WebMode,
                AudioRecording = @event.Event.AudioRecording,
                Comment = @event.Event.Comment,
                QuestionnaireId = @event.Event.QuestionnaireIdentity,
                UpgradedFromId = @event.Event.UpgradedFromId
            });

            AddRecord(@event, AssignmentExportedAction.Created, ToUpgradedFromString(@event.Event.UpgradedFromId), null, @event.Event.Comment, position: 1);
            AddRecord(@event, AssignmentExportedAction.QuantityChanged, null, ToQuantityString(@event.Event.Quantity), null, position: 2);
            AddRecord(@event, AssignmentExportedAction.WebModeChanged, null, ToWebModeString(@event.Event.WebMode), null, position: 3);
            AddRecord(@event, AssignmentExportedAction.AudioRecordingChanged, null, ToAudioRecordingString(@event.Event.AudioRecording), null, position: 4);
        }

        public void Handle(PublishedEvent<AssignmentArchived> @event)
        {
            AddRecord(@event, AssignmentExportedAction.Archived, null, null, null);
        }

        public void Handle(PublishedEvent<AssignmentUnarchived> @event)
        {
            AddRecord(@event, AssignmentExportedAction.Unarchived, null, null, null);
        }

        public void Handle(PublishedEvent<AssignmentDeleted> @event)
        {
            AddRecord(@event, AssignmentExportedAction.Deleted, null, null, null);
        }

        public void Handle(PublishedEvent<AssignmentReassigned> @event)
        {
            var assignment = GetAssignment(@event.EventSourceId);
            assignment.ResponsibleId = @event.Event.ResponsibleId;
            assignment.Comment = @event.Event.Comment;

            AddRecord(@event, AssignmentExportedAction.Reassigned, null, null, @event.Event.Comment);
        }

        public void Handle(PublishedEvent<AssignmentReceivedByTablet> @event)
        {
            AddRecord(@event, AssignmentExportedAction.ReceivedByTablet, null, null, null);
        }

        public void Handle(PublishedEvent<AssignmentAudioRecordingChanged> @event)
        {
            var assignment = GetAssignment(@event.EventSourceId);

            AddRecord(@event, AssignmentExportedAction.AudioRecordingChanged, 
                ToAudioRecordingString(assignment.AudioRecording), ToAudioRecordingString(@event.Event.AudioRecording), null);

            assignment.AudioRecording = @event.Event.AudioRecording;
        }

        public void Handle(PublishedEvent<AssignmentWebModeChanged> @event)
        {
            var assignment = GetAssignment(@event.EventSourceId);

            AddRecord(@event, AssignmentExportedAction.WebModeChanged,
                ToWebModeString(assignment.WebMode), ToWebModeString(@event.Event.WebMode), null);

            assignment.WebMode = @event.Event.WebMode;
        }

        public void Handle(PublishedEvent<AssignmentQuantityChanged> @event)
        {
            var assignment = GetAssignment(@event.EventSourceId);

            AddRecord(@event, AssignmentExportedAction.QuantityChanged,
                ToQuantityString(assignment.Quantity), ToQuantityString(@event.Event.Quantity), null);

            assignment.Quantity = @event.Event.Quantity;
        }

        private void AddRecord<T>(PublishedEvent<T> @event, AssignmentExportedAction action, 
            string? oldValue, string? newValue, string? comment, int position = 1) 
            where T : AssignmentEvent
        {
            var assignment = GetAssignment(@event.EventSourceId);

            var assignmentAction = new AssignmentAction
            {
                GlobalSequence = @event.GlobalSequence,
                Position = position,
                AssignmentId = assignment.Id,
                TimestampUtc = @event.Event.OriginDate.UtcDateTime,
                Status = action,
                OriginatorId = @event.Event.UserId,
                ResponsibleId = assignment.ResponsibleId,
                OldValue = oldValue ?? String.Empty,
                NewValue = newValue ?? String.Empty,
                Comment = comment ?? String.Empty,
            };
            dbContext.AssignmentActions.Add(assignmentAction);
        }

        private Assignment GetAssignment(Guid publicKey)
        {
            return dbContext.Assignments.Find(publicKey);
        }

        public Task SaveStateAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        private string ToQuantityString(int? quantity) => quantity.HasValue ? quantity.Value.ToString(CultureInfo.InvariantCulture) : "-1";
        private string? ToWebModeString(bool? webMode) => webMode == false ? "0" : (webMode == true ? "1" : null);
        private string ToAudioRecordingString(bool audioRecording) => audioRecording == false ? "0" : "1";
        private string? ToUpgradedFromString(int? upgradedFromId) => upgradedFromId.HasValue ? upgradedFromId.Value.ToString(CultureInfo.InvariantCulture): null; 
    }
}
