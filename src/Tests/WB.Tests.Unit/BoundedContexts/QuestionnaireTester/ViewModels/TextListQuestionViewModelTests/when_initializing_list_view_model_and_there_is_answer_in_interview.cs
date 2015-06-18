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
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Tester.TextListQuestionViewModelTests
{
    public class when_initializing_list_view_model_and_there_is_answer_in_interview : TextListQuestionViewModelTestContext
    {
        Establish context = () =>
        {
            var textListAnswer = Mock.Of<TextListAnswer>(_ => _.Answers == savedAnswers && _.IsAnswered == true);
            
            var interview = Mock.Of<IStatefulInterview>(_ 
                => _.QuestionnaireId == questionnaireId
                   && _.GetTextListAnswer(questionIdentity) == textListAnswer);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(_ => _.Get(interviewId) == interview);

            var textListQuestionModel = Mock.Of<TextListQuestionModel>(_
                => _.Id == questionIdentity.Id
                   && _.IsRosterSizeQuestion == true
                   && _.MaxAnswerCount == 5);

            var questionnaireModel = Mock.Of<QuestionnaireModel>(_ => _.Questions == new Dictionary<Guid, BaseQuestionModel> { { questionIdentity.Id, textListQuestionModel } });

            var questionnaireRepository = Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(x => x.GetById(questionnaireId) == questionnaireModel);

            listModel = CreateTextListQuestionViewModel(
                QuestionStateMock.Object,
                AnsweringViewModelMock.Object,
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository);
        };

        Because of = () =>
            listModel.Init(interviewId, questionIdentity, navigationState);

        It should_initialize_question_state = () =>
            QuestionStateMock.Verify(x => x.Init(interviewId, questionIdentity, navigationState), Times.Once);

        It should_create_list_with_5_answers = () =>
            listModel.Answers.Count.ShouldEqual(5);

        It should_create_list_with_Values_same_as_in_saved_answers = () =>
            listModel.Answers.Select(x => x.Value).ShouldContainOnly(savedAnswers.Select(x=> x.Item1));

        It should_create_list_with_Titles_same_as_in_saved_answers = () =>
            listModel.Answers.Select(x => x.Title).ShouldContainOnly(savedAnswers.Select(x => x.Item2));

        It should_set_IsAddNewItemVisible_flag_in_false = () =>
            listModel.IsAddNewItemVisible.ShouldBeFalse();

        private static TextListQuestionViewModel listModel;
        private static Identity questionIdentity = Create.Identity(Guid.Parse("11111111111111111111111111111111"), new decimal[0]);
        private static NavigationState navigationState = CreateNavigationState();
        private static readonly Mock<QuestionStateViewModel<TextListQuestionAnswered>> QuestionStateMock = new Mock<QuestionStateViewModel<TextListQuestionAnswered>>();
        private static readonly Mock<AnsweringViewModel> AnsweringViewModelMock = new Mock<AnsweringViewModel>();

        private static readonly string interviewId = "Some interviewId";
        private static readonly string questionnaireId = "Questionnaire Id";

        private static readonly Tuple<decimal, string>[] savedAnswers = new []
        {
            new Tuple<decimal, string>(1m, "Answer 1"),
            new Tuple<decimal, string>(3m, "Answer 3"),
            new Tuple<decimal, string>(4m, "Answer 5"),
            new Tuple<decimal, string>(8m, "Answer 8"),
            new Tuple<decimal, string>(9m, "Answer 9"),
        };
    }
}