using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.Infrastructure.PlainStorage;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.CascadingSingleOptionQuestionViewModelTests
{
    public class when_initializing_cascading_view_model_and_parent_question_is_answered_and_question_2_level_roster : CascadingSingleOptionQuestionViewModelTestContext
    {
        Establish context = () =>
        {
            var singleOptionAnswer = Mock.Of<SingleOptionAnswer>(_ => _.IsAnswered == false);
            var parentOptionAnswer = Mock.Of<SingleOptionAnswer>(_ => _.IsAnswered == true && _.Answer == 1);

            StatefulInterviewMock.Setup(x => x.QuestionnaireId).Returns(questionnaireId);
            StatefulInterviewMock.Setup(x => x.GetSingleOptionAnswer(questionIdentity)).Returns(singleOptionAnswer);
            StatefulInterviewMock.Setup(x => x.GetSingleOptionAnswer(parentIdentity)).Returns(parentOptionAnswer);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == StatefulInterviewMock.Object);

            var cascadingQuestionModel = Mock.Of<CascadingSingleOptionQuestionModel>(_
                => _.Id == questionIdentity.Id
                   && _.Options == Options
                   && _.CascadeFromQuestionId == parentIdentity.Id
                   && _.RosterLevelDepthOfParentQuestion == 1);

            var questionnaireModel = Mock.Of<QuestionnaireModel>(_ => _.Questions == new Dictionary<Guid, BaseQuestionModel> { { questionIdentity.Id, cascadingQuestionModel } });

            var questionnaireRepository = Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(x => x.GetById(questionnaireId) == questionnaireModel);

            cascadingModel = CreateCascadingSingleOptionQuestionViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository);
        };

        Because of = () =>
            cascadingModel.Init(interviewId, questionIdentity, navigationState);

        It should_get_answer_for_parent_question_once = () =>
            StatefulInterviewMock.Verify(x => x.GetSingleOptionAnswer(parentIdentity), Times.Once);

        It should_get_answer_for_question_once = () =>
            StatefulInterviewMock.Verify(x => x.GetSingleOptionAnswer(questionIdentity), Times.Once);

        [Ignore("Slava will fix it")]
        It should_initialize_question_state = () =>
            QuestionStateMock.Verify(x => x.Init(interviewId, questionIdentity, navigationState), Times.Once);

        It should_subscribe_for_events = () =>
            EventRegistry.Verify(x => x.Subscribe(cascadingModel), Times.Once);

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
            cascadingModel.AutoCompleteSuggestions.Select(x => x.ParentValue).ShouldContainOnly(OptionsIfParentAnswerIs1.Select(x => x.ParentValue));

        private static CascadingSingleOptionQuestionViewModel cascadingModel;
        private static readonly Mock<IStatefulInterview> StatefulInterviewMock = new Mock<IStatefulInterview>();

        private static readonly List<CascadingOptionModel> OptionsIfParentAnswerIs1 = Options.Where(x => x.ParentValue == 1).ToList();
    }
}