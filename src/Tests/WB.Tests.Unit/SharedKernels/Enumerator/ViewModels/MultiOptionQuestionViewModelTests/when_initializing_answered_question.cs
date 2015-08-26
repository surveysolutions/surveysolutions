using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionQuestionViewModelTests
{
    public class when_initializing_answered_question : MultiOptionQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            interviewId = "interview";
            questionGuid = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionId = Create.Identity(questionGuid, Empty.RosterVector);
            navigationState = Create.NavigationState();

            var questionnaire = BuildDefaultQuestionnaire(questionId);

            var multiOptionAnswer = Create.MultiOptionAnswer(questionGuid, Empty.RosterVector);
            multiOptionAnswer.SetAnswers(new[] {1m});
            
            var interview = Mock.Of<IStatefulInterview>(x => x.GetMultiOptionAnswer(questionId) == multiOptionAnswer);

            var questionnaireStorage = new Mock<IPlainKeyValueStorage<QuestionnaireModel>>();
            var interviewRepository = new Mock<IStatefulInterviewRepository>();

            questionnaireStorage.SetReturnsDefault(questionnaire);
            interviewRepository.SetReturnsDefault(interview);

            eventRegistry = new Mock<ILiteEventRegistry>();

            viewModel = CreateViewModel(questionnaireStorage: questionnaireStorage.Object, 
                interviewRepository: interviewRepository.Object,
                eventRegistry: eventRegistry.Object);
        };

        Because of = () => viewModel.Init(interviewId, questionId, navigationState);

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

