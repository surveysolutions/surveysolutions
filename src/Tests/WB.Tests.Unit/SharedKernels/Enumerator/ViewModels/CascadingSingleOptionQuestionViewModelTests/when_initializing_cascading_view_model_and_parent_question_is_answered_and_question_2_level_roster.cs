using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.Enumerator.Aggregates;

using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.CascadingSingleOptionQuestionViewModelTests
{
    [Ignore("KP-8159")]
    internal class when_initializing_cascading_view_model_and_parent_question_is_answered_and_question_2_level_roster : CascadingSingleOptionQuestionViewModelTestContext
    {
        Establish context = () =>
        {
            SetUp();
            var singleOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered == false);
            var parentOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered == true && _.GetAnswer().SelectedValue == 1);

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
        };

        Because of = () =>
            cascadingModel.Init(interviewId, questionIdentity, navigationState);

        It should_get_answer_for_parent_question_once = () =>
            StatefulInterviewMock.Verify(x => x.GetSingleOptionQuestion(parentIdentity), Times.Once);

        It should_get_answer_for_question_once = () =>
            StatefulInterviewMock.Verify(x => x.GetSingleOptionQuestion(questionIdentity), Times.Once);

        It should_initialize_question_state = () =>
            QuestionStateMock.Verify(x => x.Init(interviewId, questionIdentity, navigationState), Times.Once);

        It should_subscribe_for_events = () =>
            EventRegistry.Verify(x => x.Subscribe(cascadingModel, Moq.It.IsAny<string>()), Times.Once);

        It should_not_set_selected_object = () =>
            cascadingModel.SelectedObject.ShouldBeNull();

        It should_not_set_filter_text = () =>
            cascadingModel.FilterText.ShouldBeEmpty();

        It should_set_not_empty_list_in_AutoCompleteSuggestions = () =>
            cascadingModel.AutoCompleteSuggestions.ShouldNotBeEmpty();

        It should_set_3_items_in_AutoCompleteSuggestions = () =>
            cascadingModel.AutoCompleteSuggestions.Count.ShouldEqual(3);

        It should_create_option_models_with_specified_Texts = () =>
            cascadingModel.AutoCompleteSuggestions.Select(x => x.Text).ShouldContainOnly(OptionsIfParentAnswerIs1.Select(x => x.Title));

        It should_create_option_models_with_specified_OriginalTexts = () =>
            cascadingModel.AutoCompleteSuggestions.Select(x => x.OriginalText).ShouldContainOnly(OptionsIfParentAnswerIs1.Select(x => x.Title));

        It should_create_option_models_with_specified_values = () =>
            cascadingModel.AutoCompleteSuggestions.Select(x => Convert.ToInt32(x.Value)).ShouldContainOnly(OptionsIfParentAnswerIs1.Select(x => x.Value));

        It should_create_option_models_with_specified_ParentValues = () =>
            cascadingModel.AutoCompleteSuggestions.Select(x => Convert.ToInt32(x.ParentValue)).ShouldContainOnly(OptionsIfParentAnswerIs1.Select(x => x.ParentValue.Value));

        private static CascadingSingleOptionQuestionViewModel cascadingModel;
        private static readonly Mock<IStatefulInterview> StatefulInterviewMock = new Mock<IStatefulInterview>();

        private static readonly List<CategoricalOption> OptionsIfParentAnswerIs1 = Options.Where(x => x.ParentValue == 1).ToList();
    }
}