using System;
using System.Collections.Generic;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Assignment;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public class AssignmentDenormalizer : AbstractFunctionalEventHandler<Assignment, IReadSideStorage<Assignment>>,
        IUpdateHandler<Assignment, AssignmentCreated>,
        IUpdateHandler<Assignment, AssignmentArchived>,
        IUpdateHandler<Assignment, AssignmentUnarchived>,
        IUpdateHandler<Assignment, AssignmentReassigned>,
        IUpdateHandler<Assignment, AssignmentEmailUpdated>,
        IUpdateHandler<Assignment, AssignmentPasswordUpdated>,
        IUpdateHandler<Assignment, AssignmentWebModeUpdated>,
        IUpdateHandler<Assignment, AssignmentProtectedVariablesUpdated>
    {


        public AssignmentDenormalizer(IReadSideStorage<Assignment> readSideStorage) : base(readSideStorage)
        {
        }


        public Assignment Update(Assignment state, IPublishedEvent<AssignmentCreated> @event)
        {
            state = new Assignment(
                @event.Payload.QuestionnaireId, 
                @event.Payload.ResponsibleId,
                @event.Payload.Quantity,
                @event.Payload.IsAudioRecordingEnabled,
                @event.Payload.Email,
                @event.Payload.Password,
                @event.Payload.WebMode);
            return state;
        }

        public Assignment Update(Assignment state, IPublishedEvent<AssignmentArchived> @event)
        {
            state.Archived = true;
            state.UpdatedAtUtc = @event.Payload.OriginDate.UtcDateTime;
            return state;
        }

        public Assignment Update(Assignment state, IPublishedEvent<AssignmentUnarchived> @event)
        {
            state.Archived = false;
            state.UpdatedAtUtc = @event.Payload.OriginDate.UtcDateTime;
            return state;
        }

        public Assignment Update(Assignment state, IPublishedEvent<AssignmentReassigned> @event)
        {
            state.ResponsibleId = @event.Payload.ResponsibleId;
            state.UpdatedAtUtc = @event.Payload.OriginDate.UtcDateTime;
            return state;
        }

        public Assignment Update(Assignment state, IPublishedEvent<AssignmentEmailUpdated> @event)
        {
            state.Email = @event.Payload.Email;
            state.UpdatedAtUtc = @event.Payload.OriginDate.UtcDateTime;
            return state;
        }

        public Assignment Update(Assignment state, IPublishedEvent<AssignmentPasswordUpdated> @event)
        {
            state.Password = @event.Payload.Password;
            state.UpdatedAtUtc = @event.Payload.OriginDate.UtcDateTime;
            return state;
        }

        public Assignment Update(Assignment state, IPublishedEvent<AssignmentWebModeUpdated> @event)
        {
            state.WebMode = @event.Payload.WebMode;
            state.UpdatedAtUtc = @event.Payload.OriginDate.UtcDateTime;
            return state;
        }

        public Assignment Update(Assignment state, IPublishedEvent<AssignmentProtectedVariablesUpdated> @event)
        {
            state.ProtectedVariables = @event.Payload.ProtectedVariables;
            state.UpdatedAtUtc = @event.Payload.OriginDate.UtcDateTime;
            return state;
        }
    }
}
