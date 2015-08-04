using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Capi.EventHandler;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Capi.CascadingComboboxTests
{
    internal class when_handling_AnswersRemoved_event_of_cascading_questions : AnswerOptionsForCascadingQuestionsDenormalizerTestContext
    {
        Establish context = () =>
        {
            storage = new Mock<IReadSideRepositoryWriter<InterviewViewModel>>();
            var questionnaireDocument = CreateQuestionnaireDocument();
            var rosteerStructure = new QuestionnaireRosterStructure();

            interviewViewModel = new Mock<InterviewViewModel>(MockBehavior.Loose, interviewId, questionnaireDocument, rosteerStructure);


            interviewViewModel
                .Setup(x => x.IsQuestionReferencedByAnyCascadingQuestion(questionAId))
                .Returns(true);

            interviewViewModel
                .Setup(x => x.IsQuestionReferencedByAnyCascadingQuestion(questionBId))
                .Returns(false);

            interviewViewModel
                .Setup(x => x.IsQuestionReferencedByAnyCascadingQuestion(questionCId))
                .Returns(true);

            storage
                .Setup(x => x.GetById(interviewId.FormatGuid()))
                .Returns(interviewViewModel.Object);

            var singleOptionQuestionAnswered = new AnswersRemoved(new Identity[]
            {
                new Identity(questionAId, new decimal[0]),
                new Identity(questionBId, new decimal[]{1}),
                new Identity(questionCId, new decimal[]{0, 1})
            });

            answersRemovedEvent = ToPublishedEvent(singleOptionQuestionAnswered);
            denormalizer = CreateAnswerOptionsForCascadingQuestionsDenormalizer(interviewStorage: storage.Object);
        };

        Because of = () =>
            denormalizer.Handle(answersRemovedEvent);

        It should_pass_right_arguments_into_InterviewViewModel_for_question_A = () =>
            interviewViewModel.Verify(x => x.RemoveInstanceOfAnsweredQuestionUsableAsCascadingQuestion(questionAId, new decimal[0]), Times.Once);

        It should_not_call_remove_method_of_InterviewViewModel_for_question_B = () =>
            interviewViewModel.Verify(x => x.RemoveInstanceOfAnsweredQuestionUsableAsCascadingQuestion(questionBId, new decimal[] { 1 }), Times.Never);

        It should_pass_right_arguments_into_InterviewViewModel_for_question_C = () =>
            interviewViewModel.Verify(x => x.RemoveInstanceOfAnsweredQuestionUsableAsCascadingQuestion(questionCId, new decimal[] { 0, 1 }), Times.Once);

        It should_not_store_InterviewViewModel = () =>
            storage.Verify(x => x.Store(interviewViewModel.Object, interviewId.FormatGuid()), Times.Never);

        private static AnswerOptionsForCascadingQuestionsDenormalizer denormalizer;
        private static IPublishedEvent<AnswersRemoved> answersRemovedEvent;
        private static Guid interviewId = Guid.Parse("55555555555555555555555555555555");
        private static Guid questionAId = Guid.Parse("22222222222222222222222222222222");
        private static Guid questionBId = Guid.Parse("33333333333333333333333333333333");
        private static Guid questionCId = Guid.Parse("44444444444444444444444444444444");
        private static Mock<InterviewViewModel> interviewViewModel;
        private static Mock<IReadSideRepositoryWriter<InterviewViewModel>> storage;
    }
}