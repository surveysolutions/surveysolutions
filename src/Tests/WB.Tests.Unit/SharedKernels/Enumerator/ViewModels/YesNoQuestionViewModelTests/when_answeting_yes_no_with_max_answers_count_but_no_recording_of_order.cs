using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.YesNoQuestionViewModelTests
{
    public class when_answeting_yes_no_with_max_answers_count_but_no_recording_of_order : YesNoQuestionViewModelTestsContext
    {
        [Test]
        public void should_not_set_answers_order()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                chapterId: Id.gA,
                children: new IComposite[]
                {
                    Create.Entity.YesNoQuestion(Id.g1, answers: new int[] {1, 2}, ordered: false, maxAnswersCount: 2)
                });
            var questionnaireStorage = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);

            
            var filteredOptionsViewModel = Setup.FilteredOptionsViewModel(new List<CategoricalOption>
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
            viewModel.Handle(Create.Event.YesNoQuestionAnswered(Id.g1, new[]
            {
                Create.Entity.AnsweredYesNoOption(1, true)
            }));

            // Assert
            var firstOption = viewModel.Options.First();
            Assert.That(firstOption, Has.Property(nameof(firstOption.YesAnswerCheckedOrder)).Null);
        }
    }
}
