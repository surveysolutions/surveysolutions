using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    internal class LinkedOptionsDenormalizer : AbstractFunctionalEventHandler<InterviewLinkedQuestionOptions, IReadSideKeyValueStorage<InterviewLinkedQuestionOptions>>,
        IUpdateHandler<InterviewLinkedQuestionOptions, LinkedOptionsChanged>
    {
        public LinkedOptionsDenormalizer(IReadSideKeyValueStorage<InterviewLinkedQuestionOptions> readSideStorage) : base(readSideStorage)
        {
        }

        public InterviewLinkedQuestionOptions Update(InterviewLinkedQuestionOptions state,
            IPublishedEvent<LinkedOptionsChanged> @event)
        {
            if (state == null)
                state = new InterviewLinkedQuestionOptions();

            foreach (var changedLinkedQuestion in @event.Payload.ChangedLinkedQuestions)
            {
                state.LinkedQuestionOptions[changedLinkedQuestion.QuestionId.ToString()] = changedLinkedQuestion.Options;
            }

            return state;
        }
    }
}