using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Assignment;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public class AssignmentDenormalizer : AbstractFunctionalEventHandler<Assignment, IReadSideStorage<Assignment>>,
        IUpdateHandler<Assignment, AssignmentCreated>,
        IUpdateHandler<Assignment, AssignmentArchived>,
        IUpdateHandler<Assignment, AssignmentUnarchived>,
        IUpdateHandler<Assignment, AssignmentReassigned>,
        IUpdateHandler<Assignment, AssignmentWebModeChanged>,
        IUpdateHandler<Assignment, AssignmentAudioRecordingChanged>,
        IUpdateHandler<Assignment, AssignmentQuantityChanged>,
        IUpdateHandler<Assignment, AssignmentReceivedByTablet>
    {
        private readonly IQuestionnaireStorage questionnaireStorage;

        public AssignmentDenormalizer(IReadSideStorage<Assignment> readSideStorage,
            IQuestionnaireStorage questionnaireStorage)
            : base(readSideStorage)
        {
            this.questionnaireStorage = questionnaireStorage;
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

            state.ProtectedVariables = @event.Payload.ProtectedVariables;

            var questionnaire = questionnaireStorage.GetQuestionnaire(state.QuestionnaireId, null);
            var answers = @event.Payload.Answers;
            var identifyingQuestionIds = Enumerable.ToHashSet(questionnaire.GetPrefilledQuestions());

            var identifyingAnswers = answers
                .Where(x => identifyingQuestionIds.Contains(x.Identity.Id)).Select(a =>
                    IdentifyingAnswer.Create(state, questionnaire, a.Answer.ToString(), a.Identity))
                .ToList();

            state.IdentifyingData = identifyingAnswers;
            state.Answers = answers;
            state.UpdatedAtUtc = @event.Payload.OriginDate.UtcDateTime;


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
            state.ReceivedByTabletAtUtc = null;
            return state;
        }

        public Assignment Update(Assignment state, IPublishedEvent<AssignmentWebModeChanged> @event)
        {
            state.WebMode = @event.Payload.WebMode;
            state.UpdatedAtUtc = @event.Payload.OriginDate.UtcDateTime;
            return state;
        }

        public Assignment Update(Assignment state, IPublishedEvent<AssignmentReceivedByTablet> @event)
        {
            state.ReceivedByTabletAtUtc = @event.Payload.OriginDate.UtcDateTime;
            return state;
        }

        public Assignment Update(Assignment state, IPublishedEvent<AssignmentAudioRecordingChanged> @event)
        {
            state.IsAudioRecordingEnabled = @event.Payload.IsAudioRecordingEnabled;
            state.UpdatedAtUtc = @event.Payload.OriginDate.UtcDateTime;
            return state;
        }

        public Assignment Update(Assignment state, IPublishedEvent<AssignmentQuantityChanged> @event)
        {
            state.Quantity = @event.Payload.Quantity;
            state.UpdatedAtUtc = @event.Payload.OriginDate.UtcDateTime;
            return state;
        }
    }
}
