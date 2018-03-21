using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.CascadingSingleOptionQuestionViewModelTests
{
    internal class when_initializing_cascading_view_model_and_parent_question_is_answered_and_question_2_level_roster : CascadingSingleOptionQuestionViewModelTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            SetUp();
            var singleOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered() == false);
            var parentOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered() == true && _.GetAnswer() == Create.Entity.SingleOptionAnswer(1));

            StatefulInterviewMock.Setup(x => x.QuestionnaireIdentity).Returns(questionnaireId);
            StatefulInterviewMock.Setup(x => x.GetSingleOptionQuestion(questionIdentity)).Returns(singleOptionAnswer);
            StatefulInterviewMock.Setup(x => x.GetSingleOptionQuestion(parentIdentity)).Returns(parentOptionAnswer);

            StatefulInterviewMock.Setup(x => x.GetOptionForQuestionWithoutFilter(questionIdentity, 3, 1))
                .Returns(new CategoricalOption() { Title = "3", Value = 3, ParentValue = 1 });

            StatefulInterviewMock.Setup(x => x.GetTopFilteredOptionsForQuestion(questionIdentity, 1, string.Empty, Moq.It.IsAny<int>()))
                .Returns(Options.Where(x => x.ParentValue == 1).ToList());

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == StatefulInterviewMock.Object);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithCascadingQuestion();
            
            cascadingModel = CreateCascadingSingleOptionQuestionViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository);
            BecauseOf();
        }

        public void BecauseOf() =>
            cascadingModel.Init(interviewId, questionIdentity, navigationState);

        [NUnit.Framework.Test] public void should_get_answer_for_parent_question_once () =>
            StatefulInterviewMock.Verify(x => x.GetSingleOptionQuestion(parentIdentity), Times.Once);

        [NUnit.Framework.Test] public void should_get_answer_for_question_once () =>
            StatefulInterviewMock.Verify(x => x.GetSingleOptionQuestion(questionIdentity), Times.Once);

        [NUnit.Framework.Test] public void should_initialize_question_state () =>
            QuestionStateMock.Verify(x => x.Init(interviewId, questionIdentity, navigationState), Times.Once);

        [NUnit.Framework.Test] public void should_subscribe_for_events () =>
            EventRegistry.Verify(x => x.Subscribe(cascadingModel, Moq.It.IsAny<string>()), Times.Once);

        [NUnit.Framework.Test] public void should_not_set_filter_text () =>
            cascadingModel.FilterText.Should().BeEmpty();

        [NUnit.Framework.Test] public void should_set_not_empty_list_in_AutoCompleteSuggestions () =>
            cascadingModel.AutoCompleteSuggestions.Should().NotBeEmpty();

        [NUnit.Framework.Test] public void should_set_3_items_in_AutoCompleteSuggestions () =>
            cascadingModel.AutoCompleteSuggestions.Count.Should().Be(3);

        [NUnit.Framework.Test] public void should_create_option_models_with_specified_Texts () =>
            cascadingModel.AutoCompleteSuggestions.Should().BeEquivalentTo(OptionsIfParentAnswerIs1.Select(x => x.Title));

        private static CascadingSingleOptionQuestionViewModel cascadingModel;
        private static readonly Mock<IStatefulInterview> StatefulInterviewMock = new Mock<IStatefulInterview>();

        private static readonly List<CategoricalOption> OptionsIfParentAnswerIs1 = Options.Where(x => x.ParentValue == 1).ToList();
    }
}
