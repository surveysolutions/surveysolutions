using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.YesNoQuestionViewModelTests
{
    [TestOf(typeof(CategoricalYesNoViewModel))]
    public class YesNoQuestionViewModelTests : YesNoQuestionViewModelTestsContext
    {
        [Test]
        public void should_not_set_answers_order()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.YesNoQuestion(Id.g1, answers: new int[] { 1, 2 }, ordered: false, maxAnswersCount: 2)
            );
            var questionnaireStorage = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);

            var filteredOptionsViewModel = Abc.SetUp.FilteredOptionsViewModel(new List<CategoricalOption>
            {
                Create.Entity.CategoricalQuestionOption(1, "item1"),
                Create.Entity.CategoricalQuestionOption(2, "item2"),
            });

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview);

            var viewModel = CreateViewModel(questionnaireStorage: questionnaireStorage,
                interviewRepository: interviewRepository,
                filteredOptionsViewModel: filteredOptionsViewModel);

            viewModel.Init(interview.Id.FormatGuid(), Create.Identity(Id.g1), Create.Other.NavigationState(interviewRepository));

            // Act
            viewModel.HandleAsync(Create.Event.YesNoQuestionAnswered(Id.g1, new[]
            {
                Create.Entity.AnsweredYesNoOption(1, true)
            }));

            // Assert
            var firstOption = viewModel.Options.First();
            Assert.That(firstOption, Has.Property(nameof(firstOption.CheckedOrder)).Null);
        }

        [Test]
        public void when_toggling_answer_and_max_answers_count_reached()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.YesNoQuestion(Id.g1, answers: new int[] { 1, 2, 3 }, ordered: true, maxAnswersCount: 2)
            );
            var questionnaireStorage = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);


            var filteredOptionsViewModel = Abc.SetUp.FilteredOptionsViewModel(new List<CategoricalOption>
            {
                Create.Entity.CategoricalQuestionOption(1, "item1"),
                Create.Entity.CategoricalQuestionOption(2, "item2"),
                Create.Entity.CategoricalQuestionOption(3, "item3")
            });

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);

            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview);

            var eventRegistry = Create.Service.LiteEventRegistry();

            var viewModel = CreateViewModel(questionnaireStorage: questionnaireStorage,
                interviewRepository: interviewRepository,
                filteredOptionsViewModel: filteredOptionsViewModel,
                eventRegistry: eventRegistry);

            viewModel.Init(interview.Id.FormatGuid(), Create.Identity(Id.g1), Create.Other.NavigationState(interviewRepository));

            // act
            interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(null, Id.g1, RosterVector.Empty, new[]
            {
                Create.Entity.AnsweredYesNoOption(1, true),
                Create.Entity.AnsweredYesNoOption(2, true),
                Create.Entity.AnsweredYesNoOption(3, false)
            }));
            Abc.SetUp.ApplyInterviewEventsToViewModels(interview, eventRegistry, interview.Id);

            // assert
            viewModel.Options[0].CanBeChecked.Should().BeTrue();
            viewModel.Options[1].CanBeChecked.Should().BeTrue();
            viewModel.Options[2].CanBeChecked.Should().BeFalse();
        }
    }
}
