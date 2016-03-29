using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using Nito.AsyncEx.Synchronous;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.CascadingSingleOptionQuestionViewModelTests
{
    internal class when_initializing_cascading_view_model_and_parent_question_is_answered_and_question_2_level_roster : CascadingSingleOptionQuestionViewModelTestContext
    {
        Establish context = () =>
        {
            SetUp();
            var singleOptionAnswer = Mock.Of<SingleOptionAnswer>(_ => _.IsAnswered == false);
            var parentOptionAnswer = Mock.Of<SingleOptionAnswer>(_ => _.IsAnswered == true && _.Answer == 1);

            StatefulInterviewMock.Setup(x => x.QuestionnaireId).Returns(questionnaireId);
            StatefulInterviewMock.Setup(x => x.GetSingleOptionAnswer(questionIdentity)).Returns(singleOptionAnswer);
            StatefulInterviewMock.Setup(x => x.GetSingleOptionAnswer(parentIdentity)).Returns(parentOptionAnswer);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == StatefulInterviewMock.Object);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithCascadingQuestion();

            cascadingModel = CreateCascadingSingleOptionQuestionViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository);
        };

        Because of = () =>
            cascadingModel.InitAsync(interviewId, questionIdentity, navigationState).WaitAndUnwrapException();

        It should_get_answer_for_parent_question_once = () =>
            StatefulInterviewMock.Verify(x => x.GetSingleOptionAnswer(parentIdentity), Times.Once);

        It should_get_answer_for_question_once = () =>
            StatefulInterviewMock.Verify(x => x.GetSingleOptionAnswer(questionIdentity), Times.Once);

        It should_initialize_question_state = () =>
            QuestionStateMock.Verify(x => x.InitAsync(interviewId, questionIdentity, navigationState), Times.Once);

        It should_subscribe_for_events = () =>
            EventRegistry.Verify(x => x.Subscribe(cascadingModel, Moq.It.IsAny<string>()), Times.Once);

        It should_not_set_selected_object = () =>
            cascadingModel.SelectedObject.ShouldBeNull();

        It should_not_set_filter_text = () =>
            cascadingModel.FilterText.ShouldBeNull();

        It should_set_not_empty_list_in_AutoCompleteSuggestions = () =>
            cascadingModel.AutoCompleteSuggestions.ShouldNotBeEmpty();

        It should_set_3_items_in_AutoCompleteSuggestions = () =>
            cascadingModel.AutoCompleteSuggestions.Count.ShouldEqual(3);

        It should_create_option_models_with_specified_Texts = () =>
            cascadingModel.AutoCompleteSuggestions.Select(x => x.Text).ShouldContainOnly(OptionsIfParentAnswerIs1.Select(x => x.Title));

        It should_create_option_models_with_specified_OriginalTexts = () =>
            cascadingModel.AutoCompleteSuggestions.Select(x => x.OriginalText).ShouldContainOnly(OptionsIfParentAnswerIs1.Select(x => x.Title));

        It should_create_option_models_with_specified_values = () =>
            cascadingModel.AutoCompleteSuggestions.Select(x => x.Value).ShouldContainOnly(OptionsIfParentAnswerIs1.Select(x => x.Value));

        It should_create_option_models_with_specified_ParentValues = () =>
            cascadingModel.AutoCompleteSuggestions.Select(x => x.ParentValue).ShouldContainOnly(OptionsIfParentAnswerIs1.Select(x => x.ParentValue.Value));

        private static CascadingSingleOptionQuestionViewModel cascadingModel;
        private static readonly Mock<IStatefulInterview> StatefulInterviewMock = new Mock<IStatefulInterview>();

        private static readonly List<CategoricalQuestionOption> OptionsIfParentAnswerIs1 = Options.Where(x => x.ParentValue == 1).ToList();
    }
}