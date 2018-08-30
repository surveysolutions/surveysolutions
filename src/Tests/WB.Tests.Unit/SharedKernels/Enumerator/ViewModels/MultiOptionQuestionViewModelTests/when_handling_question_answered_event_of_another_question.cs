using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionQuestionViewModelTests
{
    internal class when_handling_question_answered_event_of_another_question : MultiOptionQuestionViewModelTestsContext
    {
        [OneTimeSetUp] 
        public void context () {
            questionGuid = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionId = Create.Entity.Identity(questionGuid, Empty.RosterVector);

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

            var multiOptionAnswer = Create.Entity.InterviewTreeMultiOptionQuestion(new[] { 2m });

            var interview = Mock.Of<IStatefulInterview>(x => x.GetMultiOptionQuestion(questionId) == multiOptionAnswer);

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
            viewModel.Handle(new MultipleOptionsQuestionAnswered(Guid.NewGuid(), Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), Empty.RosterVector, DateTime.Now, new[] { 2m, 1m }));
        }

        [Test] 
        public void should_set_not_set_checked_order_to_options () 
            => viewModel.Options.First().CheckedOrder.Should().BeNull();

        static MultiOptionQuestionViewModel viewModel;
        static Identity questionId;
        private static Guid questionGuid;
    }
}
