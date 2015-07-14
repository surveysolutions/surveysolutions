using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Tester.Implementation.Aggregates;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.Tester.Infrastructure;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.BoundedContexts.Tester.ViewModels.Questions.State;
using WB.Core.BoundedContexts.Tester.ViewModels.Questions;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Tester.TextListQuestionViewModelTests
{
    public class when_adding_new_item_in_list_view_model_and_there_is_answer_in_interview : TextListQuestionViewModelTestContext
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
                   && _.IsRosterSizeQuestion == false
                   && _.MaxAnswerCount == 5);

            var questionnaireModel = Mock.Of<QuestionnaireModel>(_ => _.Questions == new Dictionary<Guid, BaseQuestionModel> { { questionIdentity.Id, textListQuestionModel } });

            var questionnaireRepository = Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(x => x.GetById(questionnaireId) == questionnaireModel);

            var userIdentity = Mock.Of<IUserIdentity>(_ => _.UserId == userId);
            var principal = Mock.Of<IPrincipal>(_ => _.CurrentUserIdentity == userIdentity);

            listModel = CreateTextListQuestionViewModel(
                QuestionStateMock.Object,
                AnsweringViewModelMock.Object,
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository,
                principal: principal);

            listModel.Init(interviewId, questionIdentity, navigationState);
        };

        Because of = () =>
        {
            listModel.NewListItem = newListItemTitle;
            listModel.ValueChangeCommand.Execute();
        };

        It should_create_list_with_5_answers = () =>
            listModel.Answers.Count.ShouldEqual(5);

        It should_add_item_with_Title_equals_trimmed_newListItemTitle = () =>
            listModel.Answers.Last().Title.ShouldEqual(newListItemTitle.Trim());

        It should_add_new_item_with_Value_equals_9 = () =>
            listModel.Answers.Last().Value.ShouldEqual(9m);

        It should_set_IsAddNewItemVisible_flag_in_false = () =>
            listModel.IsAddNewItemVisible.ShouldBeFalse();

        It should_clear_NewListItem_field = () =>
            listModel.NewListItem.ShouldBeEmpty();

        It should_send_answer_command = () =>
            AnsweringViewModelMock.Verify(x => x.SendAnswerQuestionCommand(Moq.It.IsAny<AnswerTextListQuestionCommand>()), Times.Once);

        private static TextListQuestionViewModel listModel;
        private static Identity questionIdentity = Create.Identity(Guid.Parse("11111111111111111111111111111111"), new decimal[0]);
        private static NavigationState navigationState = Create.NavigationState();

        private static readonly Mock<QuestionStateViewModel<TextListQuestionAnswered>> QuestionStateMock = 
            new Mock<QuestionStateViewModel<TextListQuestionAnswered>>{ DefaultValue = DefaultValue.Mock };

        private static readonly Mock<AnsweringViewModel> AnsweringViewModelMock = 
            new Mock<AnsweringViewModel>() { DefaultValue = DefaultValue.Mock };

        private static readonly string interviewId = "44444444444444444444444444444444";

        private static readonly string questionnaireId = "Questionnaire Id";
        private static readonly Guid userId = Guid.Parse("ffffffffffffffffffffffffffffffff");

        private static readonly Tuple<decimal, string>[] savedAnswers = new[]
        {
            new Tuple<decimal, string>(1m, "Answer 1"),
            new Tuple<decimal, string>(3m, "Answer 3"),
            new Tuple<decimal, string>(4m, "Answer 5"),
            new Tuple<decimal, string>(8m, "Answer 8")
        };

        private static readonly string newListItemTitle = "   Hello World!      ";
    }
}