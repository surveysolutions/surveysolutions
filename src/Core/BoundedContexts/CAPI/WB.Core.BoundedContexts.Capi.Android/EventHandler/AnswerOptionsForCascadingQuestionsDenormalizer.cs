using System;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.BoundedContexts.Capi.EventHandler
{
    public class AnswerOptionsForCascadingQuestionsDenormalizer :
        IEventHandler<AnswersRemoved>,
        IEventHandler<SingleOptionQuestionAnswered>
    {
        private readonly IReadSideRepositoryWriter<InterviewViewModel> interviewStorage;
        private readonly ILogger logger;

        public AnswerOptionsForCascadingQuestionsDenormalizer(IReadSideRepositoryWriter<InterviewViewModel> interviewStorage, ILogger logger)
        {
            this.interviewStorage = interviewStorage;
            this.logger = logger;
        }

        public void Handle(IPublishedEvent<AnswersRemoved> @event)
        {
            foreach (var question in @event.Payload.Questions)
            {
                this.RemoveAnswerOptionForCascadingQuestions(@event.EventSourceId, question.Id, question.RosterVector);
            }
        }

        public void Handle(IPublishedEvent<SingleOptionQuestionAnswered> @event)
        {
            this.AddFilterForAnswerOptionForCascadingQuestions(@event.EventSourceId, @event.Payload.QuestionId, @event.Payload.PropagationVector);
        }

        private void AddFilterForAnswerOptionForCascadingQuestions(Guid interviewId, Guid questionId, decimal[] propagationVector)
        {
            this.UpdateInMemoryInterviewViewModel(interviewId, interviewViewModel =>
            {
                if (!interviewViewModel.IsQuestionReferencedByAnyLinkedQuestion(questionId))
                    return;

                interviewViewModel.AddInstanceOfAnsweredQuestionUsableAsLinkedQuestionsOption(questionId, propagationVector);
            });
        }

        private void RemoveAnswerOptionForCascadingQuestions(Guid interviewId, Guid questionId, decimal[] propagationVector)
        {
            this.UpdateInMemoryInterviewViewModel(interviewId, interviewViewModel =>
            {
                if (!interviewViewModel.IsQuestionReferencedByAnyLinkedQuestion(questionId))
                    return;

                interviewViewModel.RemoveInstanceOfAnsweredQuestionUsableAsLinkedQuestionsOption(questionId, propagationVector);
            });
        }

        private void UpdateInMemoryInterviewViewModel(Guid interviewId, Action<InterviewViewModel> update)
        {
            InterviewViewModel interview = this.interviewStorage.GetById(interviewId);

            if (interview == null)
            {
                this.logger.Warn(string.Format(
                    "Cannot apply event to interview view model because no view model with id {0} exists.",
                    interviewId.FormatGuid()));

                return;
            }

            update(interview);
        }
    }
}