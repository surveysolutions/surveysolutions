using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    internal class VariablesDenormalizer : AbstractFunctionalEventHandler<InterviewVariables
        , IReadSideKeyValueStorage<InterviewVariables>>,
        IUpdateHandler<InterviewVariables, VariablesValuesChanged>
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
                state.VariableValues[changedVariable.VariableIdentity.ToString()] = changedVariable.VariableValue;
            }

            return state;
        }
    }
}