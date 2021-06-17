using System;
using System.Collections.Generic;
using Ncqrs.Domain;
using WB.Core.SharedKernels.DataCollection.Commands.Assignment;
using WB.Core.SharedKernels.DataCollection.Events.Assignment;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.AssignmentInfrastructure;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public class AssignmentAggregateRoot : EventSourcedAggregateRoot
    {
        internal readonly AssignmentProperties properties = new AssignmentProperties();

        public override Guid EventSourceId
        {
            get => base.EventSourceId;
            protected set
            {
                base.EventSourceId = value;
                this.properties.PublicKey = value;
            }
        }

        #region Apply
        protected void Apply(AssignmentCreated @event)
        {
            this.properties.PublicKey = this.EventSourceId;
            this.properties.Id = @event.Id;
            this.properties.ResponsibleId = @event.ResponsibleId;
            this.properties.Quantity = @event.Quantity;
            this.properties.QuestionnaireId = new QuestionnaireIdentity(@event.QuestionnaireId, @event.QuestionnaireVersion);
            this.properties.AudioRecording = @event.AudioRecording;
            this.properties.Email = @event.Email;
            this.properties.Password = @event.Password;
            this.properties.WebMode = @event.WebMode;
            this.properties.Answers = @event.Answers;
            this.properties.ProtectedVariables = @event.ProtectedVariables;
            this.properties.Comment = @event.Comment;

            this.properties.CreatedAt = @event.OriginDate;
            this.properties.UpdatedAt = @event.OriginDate;
        }

        protected void Apply(AssignmentDeleted @event)
        {
            this.properties.IsDeleted = true;
        }

        protected void Apply(AssignmentAudioRecordingChanged @event)
        {
            this.properties.AudioRecording = @event.AudioRecording;
            this.properties.UpdatedAt = @event.OriginDate;
        }

        protected void Apply(AssignmentQuantityChanged @event)
        {
            this.properties.Quantity = @event.Quantity;
            this.properties.UpdatedAt = @event.OriginDate;
        }

        protected void Apply(AssignmentReassigned @event)
        {
            this.properties.ResponsibleId = @event.ResponsibleId;
            this.properties.UpdatedAt = @event.OriginDate;
            this.properties.ReceivedByTabletAt = null;

            if (!string.IsNullOrEmpty(@event.Comment))
                this.properties.Comment = @event.Comment;
        }

        protected void Apply(AssignmentArchived @event)
        {
            this.properties.Archived = true;
            this.properties.UpdatedAt = @event.OriginDate;
        }

        protected void Apply(AssignmentUnarchived @event)
        {
            this.properties.Archived = false;
            this.properties.UpdatedAt = @event.OriginDate;
        }

        protected void Apply(AssignmentReceivedByTablet @event)
        {
            this.properties.ReceivedByTabletAt = @event.OriginDate;
        }

        protected void Apply(AssignmentWebModeChanged @event)
        {
            this.properties.WebMode = @event.WebMode;
            this.properties.UpdatedAt = @event.OriginDate;
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
                command.Quantity == -1 ? null : command.Quantity,
                command.AudioRecording,
                command.Email,
                command.Password,
                command.WebMode,
                command.Answers.ToArray(),
                command.ProtectedVariables.ToArray(),
                command.Comment,
                command.UpgradedFromId));
        }

        public void DeleteAssignment(DeleteAssignment command)
        {
            ApplyEvent(new AssignmentDeleted(command.UserId, command.OriginDate));
        }

        public void Archive(ArchiveAssignment command)
        {
            AssignmentPropertiesInvariants invariants = new AssignmentPropertiesInvariants(this.properties);
            invariants.ThrowIfAssignmentDeleted();

            ApplyEvent(new AssignmentArchived(command.UserId, command.OriginDate));
        }

        public void Unarchive(UnarchiveAssignment command)
        {
            AssignmentPropertiesInvariants invariants = new AssignmentPropertiesInvariants(this.properties);
            invariants.ThrowIfAssignmentDeleted();

            ApplyEvent(new AssignmentUnarchived(command.UserId, command.OriginDate));
        }

        public void Reassign(ReassignAssignment command)
        {
            AssignmentPropertiesInvariants invariants = new AssignmentPropertiesInvariants(this.properties);
            invariants.ThrowIfAssignmentDeleted();

            ApplyEvent(new AssignmentReassigned(command.UserId, command.OriginDate, command.ResponsibleId, command.Comment));
        }

        public void MarkAssignmentAsReceivedByTablet(MarkAssignmentAsReceivedByTablet command)
        {
            AssignmentPropertiesInvariants invariants = new AssignmentPropertiesInvariants(this.properties);
            invariants.ThrowIfAssignmentDeleted();

            ApplyEvent(new AssignmentReceivedByTablet(command.UserId, command.OriginDate));
        }

        public void UpdateAssignmentAudioRecording(UpdateAssignmentAudioRecording command)
        {
            AssignmentPropertiesInvariants invariants = new AssignmentPropertiesInvariants(this.properties);
            invariants.ThrowIfAssignmentDeleted();

            ApplyEvent(new AssignmentAudioRecordingChanged(command.UserId, command.OriginDate, command.AudioRecording));
        }

        public void UpdateAssignmentQuantity(UpdateAssignmentQuantity command)
        {
            AssignmentPropertiesInvariants invariants = new AssignmentPropertiesInvariants(this.properties);
            invariants.ThrowIfAssignmentDeleted();

            var actualQuantity = command.Quantity == -1 ? null : command.Quantity;
            ApplyEvent(new AssignmentQuantityChanged(command.UserId, command.OriginDate, actualQuantity));
        }

        public void UpdateAssignmentWebMode(UpdateAssignmentWebMode command)
        {
            AssignmentPropertiesInvariants invariants = new AssignmentPropertiesInvariants(this.properties);
            invariants.ThrowIfAssignmentDeleted();

            ApplyEvent(new AssignmentWebModeChanged(command.UserId, command.OriginDate, command.WebMode));
        }

        public void UpgradeAssignment(UpgradeAssignmentCommand command)
        {
            if (properties.WebMode != false)
                ApplyEvent(new AssignmentWebModeChanged(command.UserId, command.OriginDate, false));

            ApplyEvent(new AssignmentArchived(command.UserId, command.OriginDate));
        }
    }
}
