using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.YesNoQuestionViewModelTests
{
    internal class when_handling_question_answered_event : YesNoQuestionViewModelTestsContext
    {
        [OneTimeSetUp] 
        public void context () {
            questionGuid = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionId = Create.Entity.Identity(questionGuid, Empty.RosterVector);

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

            var yesNoQuestion = Create.Entity.InterviewTreeYesNoQuestion(new[]
            {
                new AnsweredYesNoOption(5, true),
                new AnsweredYesNoOption(2, false),
            });

            var interview = Mock.Of<IStatefulInterview>(x => x.GetYesNoQuestion(questionId) == yesNoQuestion);

            var questionnaireStorage = new Mock<IQuestionnaireStorage>();
            var interviewRepository = new Mock<IStatefulInterviewRepository>();

            questionnaireStorage.SetReturnsDefault(questionnaire);
            interviewRepository.SetReturnsDefault(interview);

            viewModel = CreateViewModel(questionnaireStorage: questionnaireStorage.Object,
                interviewRepository: interviewRepository.Object,
                filteredOptionsViewModel: filteredOptionsViewModel);

            viewModel.Init("blah", questionId, Create.Other.NavigationState());
            BecauseOf();
        }

        public void BecauseOf() 
        {
            viewModel.Handle(new YesNoQuestionAnswered(Guid.NewGuid(), questionGuid, Empty.RosterVector, DateTime.Now, new []
            {
                new AnsweredYesNoOption(5, true), 
                new AnsweredYesNoOption(2, false), 
                new AnsweredYesNoOption(1, true), 
            }));
        }

        [Test] 
        public void should_set_checked_order_to_yes_options () 
        {
            viewModel.Options[4].YesAnswerCheckedOrder.Should().Be(1);
            viewModel.Options[1].YesAnswerCheckedOrder.Should().Be(null);
            viewModel.Options[0].YesAnswerCheckedOrder.Should().Be(2);
        }

        [Test] 
        public void should_mark_options_as_checked () => 
            viewModel.Options.Count(x => x.YesSelected).Should().Be(2);

        static YesNoQuestionViewModel viewModel;
        static Identity questionId;
        private static Guid questionGuid;
    }
}

