using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionQuestionViewModelTests
{
    internal class when_toggling_answer_last_availiable_option: MultiOptionQuestionViewModelTestsContext
    {
        [OneTimeSetUp] 
        public void context () {
            var options = new[]
            {
                Create.Entity.Option(1, "item1"),
                Create.Entity.Option(2, "item2"),
                Create.Entity.Option(3, "item3"),
                Create.Entity.Option(4, "item4"),
            };

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultyOptionsQuestion(Id.g1, options, maxAllowedAnswers: 2),
                Create.Entity.MultiRoster(Id.g2, rosterSizeQuestionId: Id.g1)
            );
            var questionnaireStorage = SetUp.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);

            var interview = SetUp.StatefulInterview(questionnaire);
            var interviewRepository = SetUp.StatefulInterviewRepository(interview);

            var eventRegistry = Create.Service.LiteEventRegistry();

            viewModel = CreateViewModel(
                questionnaireStorage: questionnaireStorage,
                interviewRepository: interviewRepository,
                filteredOptionsViewModel: SetUp.FilteredOptionsViewModel(options),
                eventRegistry: eventRegistry);

            interview.AnswerMultipleOptionsQuestion(Id.gF, Id.g1, RosterVector.Empty, DateTimeOffset.UtcNow, new [] { 1 });
            
            viewModel.Init(interview.Id.FormatGuid(), Create.Entity.Identity(Id.g1, Empty.RosterVector), Create.Other.NavigationState());

            var option = viewModel.Options.Second();
            option.Checked = true;
            option.CheckAnswerCommand.Execute();
            interview.AnswerMultipleOptionsQuestion(Id.gF, Id.g1, RosterVector.Empty, DateTimeOffset.UtcNow, new[] { 1, 2 });
            SetUp.ApplyInterviewEventsToViewModels(interview, eventRegistry, interview.Id);
        }

        [Test] 
        public void should_mark_all_unselected_options_as_not_available ()
        {
            viewModel.Options.ElementAt(2).Checked.Should().BeFalse();
            viewModel.Options.ElementAt(2).CanBeChecked.Should().BeFalse();

            viewModel.Options.ElementAt(3).Checked.Should().BeFalse();
            viewModel.Options.ElementAt(3).CanBeChecked.Should().BeFalse();
        }

        static CategoricalMultiViewModel viewModel;
    }
}
