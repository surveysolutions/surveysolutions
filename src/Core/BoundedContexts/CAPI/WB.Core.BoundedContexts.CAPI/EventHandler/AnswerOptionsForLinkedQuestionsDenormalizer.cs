using System;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.BoundedContexts.Capi.EventHandler
{
    public class AnswerOptionsForLinkedQuestionsDenormalizer :
        IEventHandler<AnswersRemoved>,
        IEventHandler<TextQuestionAnswered>,
        IEventHandler<NumericIntegerQuestionAnswered>,
        IEventHandler<NumericRealQuestionAnswered>,
        IEventHandler<DateTimeQuestionAnswered>
    {
        private readonly IReadSideRepositoryWriter<InterviewViewModel> interviewStorage;
        private readonly ILogger logger;

        public AnswerOptionsForLinkedQuestionsDenormalizer(IReadSideRepositoryWriter<InterviewViewModel> interviewStorage, ILogger logger)
        {
            this.interviewStorage = interviewStorage;
            this.logger = logger;
        }

        public void Handle(IPublishedEvent<AnswersRemoved> @event)
        {
            foreach (var question in @event.Payload.Questions)
            {
                this.RemoveAnswerOptionForLinkedQuestions(@event.EventSourceId, question.Id, question.RosterVector);
            }
        }

        public void Handle(IPublishedEvent<TextQuestionAnswered> @event)
        {
            this.AddAnswerOptionForLinkedQuestions(@event.EventSourceId, @event.Payload.QuestionId, @event.Payload.PropagationVector);
        }

        public void Handle(IPublishedEvent<NumericIntegerQuestionAnswered> @event)
        {
            this.AddAnswerOptionForLinkedQuestions(@event.EventSourceId, @event.Payload.QuestionId, @event.Payload.PropagationVector);
        }

        public void Handle(IPublishedEvent<NumericRealQuestionAnswered> @event)
        {
            this.AddAnswerOptionForLinkedQuestions(@event.EventSourceId, @event.Payload.QuestionId, @event.Payload.PropagationVector);
        }

        public void Handle(IPublishedEvent<DateTimeQuestionAnswered> @event)
        {
            this.AddAnswerOptionForLinkedQuestions(@event.EventSourceId, @event.Payload.QuestionId, @event.Payload.PropagationVector);
        }

        private void AddAnswerOptionForLinkedQuestions(Guid interviewId, Guid questionId, decimal[] propagationVector)
        {
            this.UpdateInMemoryInterviewViewModel(interviewId, interviewViewModel =>
            {
                if (!interviewViewModel.IsQuestionReferencedByAnyLinkedQuestion(questionId))
                    return;

                interviewViewModel.AddInstanceOfAnsweredQuestionUsableAsLinkedQuestionsOption(questionId, propagationVector);
            });
        }

        private void RemoveAnswerOptionForLinkedQuestions(Guid interviewId, Guid questionId, decimal[] propagationVector)
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