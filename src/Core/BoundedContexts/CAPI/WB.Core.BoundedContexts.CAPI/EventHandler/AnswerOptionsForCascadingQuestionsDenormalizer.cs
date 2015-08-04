using System;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
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
                Guid questionId = question.Id;
                this.UpdateInMemoryInterviewViewModel(@event.EventSourceId, interviewViewModel =>
                {
                    if (!interviewViewModel.IsQuestionReferencedByAnyCascadingQuestion(questionId))
                        return;

                    interviewViewModel.RemoveInstanceOfAnsweredQuestionUsableAsCascadingQuestion(questionId, question.RosterVector);
                });
            }
        }

        public void Handle(IPublishedEvent<SingleOptionQuestionAnswered> @event)
        {
            Guid questionId = @event.Payload.QuestionId;
            this.UpdateInMemoryInterviewViewModel(@event.EventSourceId, interviewViewModel =>
            {
                if (!interviewViewModel.IsQuestionReferencedByAnyCascadingQuestion(questionId))
                    return;

                interviewViewModel.AddInstanceOfAnsweredQuestionUsableAsCascadingQuestion(questionId, @event.Payload.PropagationVector, @event.Payload.SelectedValue);
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