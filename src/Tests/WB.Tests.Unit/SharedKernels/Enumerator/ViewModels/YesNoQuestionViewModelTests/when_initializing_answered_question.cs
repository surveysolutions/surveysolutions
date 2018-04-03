using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.YesNoQuestionViewModelTests
{
    internal class when_initializing_answered_question : YesNoQuestionViewModelTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            interviewId = "interview";
            questionGuid = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionId = Create.Entity.Identity(questionGuid, Empty.RosterVector);
            navigationState = Create.Other.NavigationState();

            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.ShouldQuestionRecordAnswersOrder(questionId.Id) == true
                && _.GetMaxSelectedAnswerOptions(questionId.Id) == null
                && _.IsRosterSizeQuestion(questionId.Id) == false
            );

            var filteredOptionsViewModel = Setup.FilteredOptionsViewModel(new List<CategoricalOption>
            {
                Create.Entity.CategoricalQuestionOption(1, "item1"),
                Create.Entity.CategoricalQuestionOption(2, "item2"),
                Create.Entity.CategoricalQuestionOption(3, "item3"),
                Create.Entity.CategoricalQuestionOption(4, "item4"),
                Create.Entity.CategoricalQuestionOption(5, "item5"),
            });

            var yesNoAnswer = Create.Entity.InterviewTreeYesNoQuestion(new[]
            {
                new AnsweredYesNoOption(5, true),
                new AnsweredYesNoOption(2, false),
            });

            var interview = Mock.Of<IStatefulInterview>(x => x.GetYesNoQuestion(questionId) == yesNoAnswer);

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

        [NUnit.Framework.Test] public void should_build_options () => viewModel.Options.Count.Should().Be(5);

        [NUnit.Framework.Test] public void should_mark_answered_options_as_checked () 
        {
            var lastOption = viewModel.Options.Last();
            lastOption.YesSelected.Should().BeTrue();
            lastOption.Title.Should().Be("item5");
            lastOption.YesAnswerCheckedOrder.Should().Be(1);
            lastOption.Value.Should().Be(5m);
        }

        [NUnit.Framework.Test] public void should_subscribe_model_in_events_registry () => eventRegistry.Verify(x => x.Subscribe(viewModel, Moq.It.IsAny<string>()));

        static YesNoQuestionViewModel viewModel;
        static string interviewId;
        static Identity questionId;
        static NavigationState navigationState;
        private static Guid questionGuid;
        private static Mock<ILiteEventRegistry> eventRegistry;
    }
}

