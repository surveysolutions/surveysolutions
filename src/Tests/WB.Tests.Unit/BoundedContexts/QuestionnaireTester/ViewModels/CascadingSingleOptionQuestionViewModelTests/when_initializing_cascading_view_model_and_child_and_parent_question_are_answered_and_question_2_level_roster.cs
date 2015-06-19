using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.CascadingSingleOptionQuestionViewModelTests
{
    public class when_initializing_cascading_view_model_and_child_and_parent_question_are_answered_and_question_2_level_roster : CascadingSingleOptionQuestionViewModelTestContext
    {
        Establish context = () =>
        {
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
                   && _.RosterLevelDeepOfParentQuestion == 1);

            var questionnaireModel = Mock.Of<QuestionnaireModel>(_ => _.Questions == new Dictionary<Guid, BaseQuestionModel> { { questionIdentity.Id, cascadingQuestionModel } });

            var questionnaireRepository = Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(x => x.GetById(questionnaireId) == questionnaireModel);

            cascadingModel = CreateCascadingSingleOptionQuestionViewModel(
                QuestionStateMock.Object,
                AnsweringViewModelMock.Object,
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository,
                eventRegistry: EventRegistry.Object);
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

        It should_create_6_option_models = () =>
            cascadingModel.Options.Count.ShouldEqual(6);

        It should_create_option_models_with_specified_Texts = () =>
            cascadingModel.Options.Select(x => x.Text).ShouldContainOnly(Options.Select(x => x.Title));

        It should_create_option_models_with_specified_OriginalTexts = () =>
            cascadingModel.Options.Select(x => x.OriginalText).ShouldContainOnly(Options.Select(x => x.Title));

        It should_create_option_models_with_specified_values = () =>
            cascadingModel.Options.Select(x => x.Value).ShouldContainOnly(Options.Select(x => x.Value));

        It should_create_option_models_with_specified_ParentValues = () =>
            cascadingModel.Options.Select(x => x.ParentValue).ShouldContainOnly(Options.Select(x => x.ParentValue));
        
        It should_not_set_filter_text = () =>
            cascadingModel.FilterText.ShouldBeNull();

        It should_set_empty_list_in_AutoCompleteSuggestions = () =>
            cascadingModel.AutoCompleteSuggestions.ShouldBeEmpty();

        private static CascadingSingleOptionQuestionViewModel cascadingModel;
        private static Identity questionIdentity = Create.Identity(Guid.Parse("11111111111111111111111111111111"), new decimal[] { 1, 2 });
        private static Identity parentIdentity = Create.Identity(Guid.Parse("22222222222222222222222222222222"), new decimal[] { 1 });
        private static NavigationState navigationState = Create.NavigationState();
        private static readonly Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> QuestionStateMock = new Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>>();
        private static readonly Mock<AnsweringViewModel> AnsweringViewModelMock = new Mock<AnsweringViewModel>();
        private static readonly Mock<ILiteEventRegistry> EventRegistry = new Mock<ILiteEventRegistry>();
        private static readonly Mock<IStatefulInterview> StatefulInterviewMock = new Mock<IStatefulInterview>();

        private static readonly List<CascadingOptionModel> Options = new List<CascadingOptionModel>
                                                                     {
                                                                         Create.CascadingOptionModel(1, "title 1", 1),
                                                                         Create.CascadingOptionModel(2, "title 2", 1),
                                                                         Create.CascadingOptionModel(3, "title 3", 1),
                                                                         Create.CascadingOptionModel(4, "title 4", 2),
                                                                         Create.CascadingOptionModel(5, "title 5", 2),
                                                                         Create.CascadingOptionModel(6, "title 6", 2)
                                                                     };

        private static readonly string interviewId = "Some interviewId";
        private static readonly string questionnaireId = "Questionnaire Id";

        private static readonly int answerOnChildQuestion = 3;
    }
}