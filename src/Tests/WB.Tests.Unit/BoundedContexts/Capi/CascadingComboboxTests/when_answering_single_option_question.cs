using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Capi.EventHandler;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Capi.CascadingComboboxTests
{
    internal class when_answering_single_option_question : AnswerOptionsForCascadingQuestionsDenormalizerTestContext
    {
        Establish context = () =>
        {
            storage = new Mock<IReadSideRepositoryWriter<InterviewViewModel>>();
            var questionnaireDocument = CreateQuestionnaireDocument();
            var rosteerStructure = new QuestionnaireRosterStructure();

            interviewViewModel = new Mock<InterviewViewModel>(MockBehavior.Loose, interviewId, questionnaireDocument, rosteerStructure);


            interviewViewModel
                .Setup(x => x.IsQuestionReferencedByAnyCascadingQuestion(questionId))
                .Returns(true);

            storage
                .Setup(x => x.GetById(interviewId.FormatGuid()))
                .Returns(interviewViewModel.Object);

            var singleOptionQuestionAnswered = new SingleOptionQuestionAnswered(userId, questionId, new decimal[0], DateTime.Now, 8);
            singleOptionAnsweredEvent = ToPublishedEvent(singleOptionQuestionAnswered);
            denormalizer = CreateAnswerOptionsForCascadingQuestionsDenormalizer(interviewStorage: storage.Object);
        };

        Because of = () => 
            denormalizer.Handle(singleOptionAnsweredEvent);

        It should_pass_right_arguments_into_InterviewViewModel = () =>
            interviewViewModel.Verify(x => x.AddInstanceOfAnsweredQuestionUsableAsCascadingQuestion(questionId, new decimal[0], 8), Times.Once);

        It should_not_store_InterviewViewModel = () =>
            storage.Verify(x => x.Store(interviewViewModel.Object ,interviewId.FormatGuid()), Times.Never);

        private static AnswerOptionsForCascadingQuestionsDenormalizer denormalizer;
        private static IPublishedEvent<SingleOptionQuestionAnswered> singleOptionAnsweredEvent;
        private static Guid interviewId = Guid.Parse("55555555555555555555555555555555");
        private static Guid userId = Guid.Parse("11111111111111111111111111111111");
        private static Guid questionId = Guid.Parse("22222222222222222222222222222222");
        private static Mock<InterviewViewModel> interviewViewModel;
        private static Mock<IReadSideRepositoryWriter<InterviewViewModel>> storage;
    }
}