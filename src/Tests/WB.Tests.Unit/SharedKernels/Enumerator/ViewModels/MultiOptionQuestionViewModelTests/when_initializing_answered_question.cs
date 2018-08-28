using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionQuestionViewModelTests
{
    internal class when_initializing_answered_question : MultiOptionQuestionViewModelTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            interviewId = "interview";
            questionGuid = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionId = Create.Entity.Identity(questionGuid, Empty.RosterVector);
            navigationState = Create.Other.NavigationState();

            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.ShouldQuestionRecordAnswersOrder(questionId.Id) == true
                && _.GetMaxSelectedAnswerOptions(questionId.Id) == 1
                && _.IsRosterSizeQuestion(questionId.Id) == false
            );

            var filteredOptionsViewModel = Setup.FilteredOptionsViewModel(new List<CategoricalOption>
            {
                Create.Entity.CategoricalQuestionOption(1, "item1"),
                Create.Entity.CategoricalQuestionOption(2, "item2"),
            });

            var multiOptionAnswer = Create.Entity.InterviewTreeMultiOptionQuestion(new[] { 1m });
            
            var interview = Mock.Of<IStatefulInterview>(x => x.GetMultiOptionQuestion(questionId) == multiOptionAnswer);

            var questionnaireStorage = new Mock<IQuestionnaireStorage>();
            var interviewRepository = new Mock<IStatefulInterviewRepository>();

            questionnaireStorage.SetReturnsDefault(questionnaire);
            interviewRepository.SetReturnsDefault(interview);

            eventRegistry = new Mock<ILiteEventRegistry>();

            viewModel = CreateViewModel(questionnaireStorage: questionnaireStorage.Object, 
                interviewRepository: interviewRepository.Object,
                eventRegistry: eventRegistry.Object,
                filteredOptionsViewModel: filteredOptionsViewModel);
            BecauseOf();
        }

        public void BecauseOf() => viewModel.Init(interviewId, questionId, navigationState);

        [NUnit.Framework.Test] public void should_build_options () => viewModel.Options.Count.Should().Be(2);

        [NUnit.Framework.Test] public void should_mark_answered_options_as_checked () 
        {
            var firstOption = viewModel.Options.First();
            firstOption.Checked.Should().BeTrue();
            firstOption.Title.Should().Be("item1");
            firstOption.CheckedOrder.Should().Be(1);
            firstOption.Value.Should().Be(1);
        }

        [NUnit.Framework.Test] public void should_subscribe_model_in_events_registry () => eventRegistry.Verify(x => x.Subscribe(viewModel, Moq.It.IsAny<string>()));

        static MultiOptionQuestionViewModel viewModel;
        static string interviewId;
        static Identity questionId;
        static NavigationState navigationState;
        private static Guid questionGuid;
        private static Mock<ILiteEventRegistry> eventRegistry;
    }
}

