using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using Nito.AsyncEx.Synchronous;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionQuestionViewModelTests
{
    internal class when_initializing_answered_question : MultiOptionQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            interviewId = "interview";
            questionGuid = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionId = Create.Identity(questionGuid, Empty.RosterVector);
            navigationState = Create.NavigationState();

            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.ShouldQuestionRecordAnswersOrder(questionId.Id) == true
                && _.GetMaxSelectedAnswerOptions(questionId.Id) == 1
                && _.ShouldQuestionSpecifyRosterSize(questionId.Id) == false
                && _.GetAnswerOptionsAsValues(questionId.Id) == new decimal[] { 1, 2 }
                && _.GetAnswerOptionTitle(questionId.Id, 1) == "item1"
                && _.GetAnswerOptionTitle(questionId.Id, 2) == "item2"
            );

            var multiOptionAnswer = Create.MultiOptionAnswer(questionGuid, Empty.RosterVector);
            multiOptionAnswer.SetAnswers(new[] {1m});
            
            var interview = Mock.Of<IStatefulInterview>(x => x.GetMultiOptionAnswer(questionId) == multiOptionAnswer);

            var questionnaireStorage = new Mock<IPlainQuestionnaireRepository>();
            var interviewRepository = new Mock<IStatefulInterviewRepository>();

            questionnaireStorage.SetReturnsDefault(questionnaire);
            interviewRepository.SetReturnsDefault(interview);

            eventRegistry = new Mock<ILiteEventRegistry>();

            viewModel = CreateViewModel(questionnaireStorage: questionnaireStorage.Object, 
                interviewRepository: interviewRepository.Object,
                eventRegistry: eventRegistry.Object);
        };

        Because of = () => viewModel.InitAsync(interviewId, questionId, navigationState).WaitAndUnwrapException();

        It should_build_options = () => viewModel.Options.Count.ShouldEqual(2);

        It should_mark_answered_options_as_checked = () =>
        {
            var firstOption = viewModel.Options.First();
            firstOption.Checked.ShouldBeTrue();
            firstOption.Title.ShouldEqual("item1");
            firstOption.CheckedOrder.ShouldEqual(1);
            firstOption.Value.ShouldEqual(1m);
        };

        It should_subscribe_model_in_events_registry = () => eventRegistry.Verify(x => x.Subscribe(viewModel, Moq.It.IsAny<string>()));

        static MultiOptionQuestionViewModel viewModel;
        static string interviewId;
        static Identity questionId;
        static NavigationState navigationState;
        private static Guid questionGuid;
        private static Mock<ILiteEventRegistry> eventRegistry;
    }
}

