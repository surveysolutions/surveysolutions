using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.CascadingSingleOptionQuestionViewModelTests
{
    internal class when_setting_FilterText_in_not_empty_value: CascadingSingleOptionQuestionViewModelTestContext
    {
        [OneTimeSetUp]
        public async Task context() 
        {
            SetUp();

            var childAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered() == true && _.GetAnswer() == Create.Entity.SingleOptionAnswer(3));
            var parentOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered() == true && _.GetAnswer() == Create.Entity.SingleOptionAnswer(1));

            var interview = new Mock<IStatefulInterview>();

            interview.Setup(x => x.QuestionnaireIdentity).Returns(questionnaireId);
            interview.Setup(x => x.GetSingleOptionQuestion(questionIdentity)).Returns(childAnswer);
            interview.Setup(x => x.GetSingleOptionQuestion(parentIdentity)).Returns(parentOptionAnswer);
            interview.Setup(x => x.GetOptionForQuestionWithoutFilter(questionIdentity, 3, 1)).Returns(new CategoricalOption() { Title = "3", Value = 3, ParentValue = 1 });
            interview.Setup(x => x.GetTopFilteredOptionsForQuestion(Moq.It.IsAny<Identity>(), Moq.It.IsAny<int?>(), Moq.It.IsAny<string>(), Moq.It.IsAny<int>()))
                .Returns((Identity identity, int? value, string filter, int count) => Options.Where(x => x.ParentValue == value && x.Title.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).ToList());


            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == interview.Object);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithCascadingQuestion();

            cascadingModel = CreateCascadingSingleOptionQuestionViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository);

            cascadingModel.Init(interviewId, questionIdentity, navigationState);
            await Becauseof();
        }

        public Task Becauseof() => cascadingModel.FilterCommand.ExecuteAsync("3");

        [Test]
        public void should_not_set_filter_text() =>
            cascadingModel.FilterText.Should().NotBeNull();

        [Test]
        public void should_set_empty_list_in_AutoCompleteSuggestions() =>
            cascadingModel.AutoCompleteSuggestions.Should().NotBeEmpty();

        private static CascadingSingleOptionQuestionViewModel cascadingModel;
    }
}