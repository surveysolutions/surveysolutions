using System;
using System.Collections.Generic;
using Ncqrs.Domain;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Assignment;
using WB.Core.SharedKernels.DataCollection.Events.Assignment;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public class AssignmentAggregateRoot : AggregateRootMappedByConvention
    {
        public int Id { get; protected set; }

        public Guid ResponsibleId { get; protected set; }

        public int? Quantity { get; protected set; }

        public bool Archived { get; protected set; }

        public DateTimeOffset CreatedAt { get; protected set; }

        public DateTimeOffset UpdatedAt { get; protected set; }

        public DateTimeOffset? ReceivedByTabletAt { get; protected set; }

        public QuestionnaireIdentity QuestionnaireId { get; set; }

        public bool IsAudioRecordingEnabled { get; protected set; }

        public string Email { get; protected set; }

        public string Password { get; protected set; }

        public bool? WebMode { get; protected set; }

        public IList<InterviewAnswer> Answers { get; protected set; }

        public IList<string> ProtectedVariables { get; protected set; }


        #region Apply
        protected void Apply(AssignmentCreated @event)
        {
            this.Id = @event.Id;
            this.ResponsibleId = @event.ResponsibleId;
            this.Quantity = @event.Quantity;
            this.QuestionnaireId = @event.QuestionnaireIdentity;
            this.IsAudioRecordingEnabled = @event.IsAudioRecordingEnabled;
            this.Email = @event.Email;
            this.Password = @event.Password;
            this.WebMode = @event.WebMode;
            this.Answers = @event.Answers;
            this.ProtectedVariables = @event.ProtectedVariables;

            this.CreatedAt = @event.OriginDate;
            this.UpdatedAt = @event.OriginDate;
        }

        protected void Apply(AssignmentAudioRecordingChanged @event)
        {
            this.IsAudioRecordingEnabled = @event.IsAudioRecordingEnabled;
            this.UpdatedAt = @event.OriginDate;
        }

        protected void Apply(AssignmentQuantityChanged @event)
        {
            this.Quantity = @event.Quantity;
            this.UpdatedAt = @event.OriginDate;
        }

        protected void Apply(AssignmentReassigned @event)
        {
            this.ResponsibleId = @event.ResponsibleId;
            this.UpdatedAt = @event.OriginDate;
            this.ReceivedByTabletAt = null;
        }

        protected void Apply(AssignmentArchived @event)
        {
            this.Archived = true;
            this.UpdatedAt = @event.OriginDate;
        }

        protected void Apply(AssignmentUnarchived @event)
        {
            this.Archived = false;
            this.UpdatedAt = @event.OriginDate;
        }

        protected void Apply(AssignmentReceivedByTablet @event)
        {
            this.ReceivedByTabletAt = @event.OriginDate;
        }

        protected void Apply(AssignmentWebModeChanged @event)
        {
            this.WebMode = @event.WebMode;
            this.UpdatedAt = @event.OriginDate;
        }

        #endregion

        public void CreateAssignment(CreateAssignment command)
        {
            ApplyEvent(new AssignmentCreated(
                command.UserId,
                command.Id,
                command.OriginDate,
                command.QuestionnaireId.QuestionnaireId,
                command.QuestionnaireId.Version,
                command.ResponsibleId,
                command.Quantity,
                command.IsAudioRecordingEnabled,
                command.Email,
                command.Password,
                command.WebMode,
                command.Answers.ToArray(),
                command.ProtectedVariables.ToArray()));
        }

        public void Archive(ArchiveAssignment command)
        {
            ApplyEvent(new AssignmentArchived(command.UserId, command.OriginDate));
        }

        public void Unarchive(UnarchiveAssignment command)
        {
            ApplyEvent(new AssignmentUnarchived(command.UserId, command.OriginDate));
        }

        public void Reassign(ReassignAssignment command)
        {
            ApplyEvent(new AssignmentReassigned(command.UserId, command.OriginDate, command.ResponsibleId));
        }

        public void MarkAssignmentAsReceivedByTablet(MarkAssignmentAsReceivedByTablet command)
        {
            ApplyEvent(new AssignmentReceivedByTablet(command.UserId, command.OriginDate));
        }

        public void UpdateAssignmentAudioRecording(UpdateAssignmentAudioRecording command)
        {
            ApplyEvent(new AssignmentAudioRecordingChanged(command.UserId, command.OriginDate, command.IsAudioRecordingEnabled));
        }

        public void UpdateAssignmentQuantity(UpdateAssignmentQuantity command)
        {
            ApplyEvent(new AssignmentQuantityChanged(command.UserId, command.OriginDate, command.Quantity));
        }

        public void UpdateAssignmentWebMode(UpdateAssignmentWebMode command)
        {
            ApplyEvent(new AssignmentWebModeChanged(command.UserId, command.OriginDate, command.WebMode));
        }
    }
}
