using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    internal class VariablesDenormalizer : AbstractFunctionalEventHandler<InterviewVariables, IReadSideKeyValueStorage<InterviewVariables>>,
        IUpdateHandler<InterviewVariables, VariablesValuesChanged>,
        IUpdateHandler<InterviewVariables, VariablesDisabled>,
        IUpdateHandler<InterviewVariables, VariablesEnabled>
    {
        public VariablesDenormalizer(IReadSideKeyValueStorage<InterviewVariables> readSideStorage) : base(readSideStorage)
        {
        }

        public InterviewVariables Update(InterviewVariables state, IPublishedEvent<VariablesValuesChanged> @event)
        {
            if (state == null)
                state = new InterviewVariables();

            foreach (var changedVariable in @event.Payload.ChangedVariables)
            {
                state.VariableValues[new InterviewItemId(changedVariable.Identity.Id, changedVariable.Identity.RosterVector)] = changedVariable.NewValue;
            }

            return state;
        }

        public InterviewVariables Update(InterviewVariables state, IPublishedEvent<VariablesDisabled> @event)
        {
            if (state == null)
                state = new InterviewVariables();

            foreach (var disabledVariable in @event.Payload.Variables)
            {
                state.DisabledVariables.Add(new InterviewItemId(disabledVariable.Id, disabledVariable.RosterVector));
            }

            return state;
        }

        public InterviewVariables Update(InterviewVariables state, IPublishedEvent<VariablesEnabled> @event)
        {
            if (state == null)
                state = new InterviewVariables();

            foreach (var enabledVariable in @event.Payload.Variables)
            {
                state.DisabledVariables.Remove(new InterviewItemId(enabledVariable.Id, enabledVariable.RosterVector));
            }

            return state;
        }
    }
}