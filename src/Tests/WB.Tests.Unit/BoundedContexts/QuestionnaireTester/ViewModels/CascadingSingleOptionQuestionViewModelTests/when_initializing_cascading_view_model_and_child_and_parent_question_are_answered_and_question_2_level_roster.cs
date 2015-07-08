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
    public class when_initializing_cascading_view_model_and_child_and_parent_question_are_answered_and_question_2_level_roster : CascadingSingleOptionQuestionViewModelTestContext
    {
        Establish context = () =>
        {
            SetUp();
            var childAnswer = Mock.Of<SingleOptionAnswer>(_ => _.IsAnswered == true && _.Answer == answerOnChildQuestion);
            var parentOptionAnswer = Mock.Of<SingleOptionAnswer>(_ => _.IsAnswered == true && _.Answer == 1);

            StatefulInterviewMock.Setup(x => x.QuestionnaireId).Returns(questionnaireId);
            StatefulInterviewMock.Setup(x => x.GetSingleOptionAnswer(questionIdentity)).Returns(childAnswer);
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

        It should_set_not_null_selected_object = () => 
            cascadingModel.SelectedObject.ShouldNotBeNull();

        It should_set_selected_object = () => 
            cascadingModel.SelectedObject.Value.ShouldEqual(answerOnChildQuestion);

        It should_get_answer_for_question_once = () =>
            StatefulInterviewMock.Verify(x => x.GetSingleOptionAnswer(questionIdentity), Times.Once);

        It should_initialize_question_state = () =>
            QuestionStateMock.Verify(x => x.Init(interviewId, questionIdentity, navigationState), Times.Once);

        It should_subscribe_for_events = () =>
            EventRegistry.Verify(x => x.Subscribe(cascadingModel), Times.Once);
        
        It should_set_filter_text = () =>
            cascadingModel.FilterText.ShouldEqual(cascadingModel.SelectedObject.OriginalText);

        It should_set_1_item_list_in_AutoCompleteSuggestions = () =>
            cascadingModel.AutoCompleteSuggestions.Count.ShouldEqual(1);

        It should_format_first_option_in_AutoCompleteSuggestions = () =>
        {
            var firstOption = cascadingModel.AutoCompleteSuggestions.ElementAt(0);
            firstOption.Text.ShouldContain("title klo 3");
            firstOption.Value.ShouldEqual(answerOnChildQuestion);
            firstOption.ParentValue.ShouldEqual(1);
            firstOption.OriginalText.ShouldContain("title klo 3");
        };

        private static CascadingSingleOptionQuestionViewModel cascadingModel;
        private static readonly Mock<IStatefulInterview> StatefulInterviewMock = new Mock<IStatefulInterview>();
        private const int answerOnChildQuestion = 3;
    }
}