using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.CascadingSingleOptionQuestionViewModelTests
{
    internal class CategoricalComboboxAutocompleteViewModelTest : CategoricalComboboxAutocompleteViewModelTestContext
    {
        [Test]
        public async Task when_setting_FilterText_in_not_empty_value()
        {
            CategoricalComboboxAutocompleteViewModel comboModel;

            SetUp();

            var childAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ =>
                _.IsAnswered() == true && _.GetAnswer() == Create.Entity.SingleOptionAnswer(3));
            var parentOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ =>
                _.IsAnswered() == true && _.GetAnswer() == Create.Entity.SingleOptionAnswer(1));

            var interview = new Mock<IStatefulInterview>();

            interview.Setup(x => x.QuestionnaireIdentity).Returns(questionnaireId);
            interview.Setup(x => x.GetSingleOptionQuestion(questionIdentity)).Returns(childAnswer);
            interview.Setup(x => x.GetSingleOptionQuestion(parentIdentity)).Returns(parentOptionAnswer);
            interview.Setup(x => x.GetOptionForQuestionWithoutFilter(questionIdentity, 3, 1))
                .Returns(new CategoricalOption() {Title = "3", Value = 3, ParentValue = 1});
            interview.Setup(x => x.GetTopFilteredOptionsForQuestion(Moq.It.IsAny<Identity>(), Moq.It.IsAny<int?>(),
                    Moq.It.IsAny<string>(), Moq.It.IsAny<int>(), It.IsAny<int[]>()))
                .Returns((Identity identity, int? value, string filter, int count, int[] excludedOptions) => Options.Where(x =>
                        x.ParentValue == 1 && x.Title.IndexOf(filter ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList());

            var interviewRepository = Create.Storage.InterviewRepository(interview.Object);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithCascadingQuestion();

            var filtered = new FilteredOptionsViewModel(
                questionnaireRepository ?? Mock.Of<IQuestionnaireStorage>(),
                interviewRepository,
                Mock.Of<AnswerNotifier>(),
                Mock.Of<ILogger>());

            filtered.Init(interviewId, questionIdentity, 50);

            comboModel = CreateCategoricalComboboxAutocompleteViewModel(
                Create.ViewModel.QuestionState<SingleOptionQuestionAnswered>(), filtered);
            
            comboModel.Init(interviewId, questionIdentity, navigationState);

            //act
            await comboModel.FilterCommand.ExecuteAsync("3");

            comboModel.FilterText.Should().NotBeNull();
            comboModel.AutoCompleteSuggestions.Should().NotBeEmpty();
        }
    }
}
