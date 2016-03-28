using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using Nito.AsyncEx.Synchronous;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.TextListQuestionViewModelTests
{
    internal class when_adding_new_item_in_list_view_model_with_39_items_and_this_questions_is_roster_size : TextListQuestionViewModelTestContext
    {
        Establish context = () =>
        {
            var textListAnswer = Mock.Of<TextListAnswer>(_ => _.Answers == savedAnswers && _.IsAnswered == true);

            var interview = Mock.Of<IStatefulInterview>(_
                => _.QuestionnaireId == questionnaireId
                   && _.GetTextListAnswer(questionIdentity) == textListAnswer);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(_ => _.Get(interviewId) == interview);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithListQuestion(isRosterSizeQuestion: true, maxAnswerCount : null);

            var userIdentity = Mock.Of<IUserIdentity>(_ => _.UserId == userId);
            var principal = Mock.Of<IPrincipal>(_ => _.CurrentUserIdentity == userIdentity);

            listModel = CreateTextListQuestionViewModel(
                QuestionStateMock.Object,
                AnsweringViewModelMock.Object,
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository,
                principal: principal);

            listModel.InitAsync(interviewId, questionIdentity, navigationState).WaitAndUnwrapException();
        };

        Because of = () =>
        {
            listModel.NewListItem = newListItemTitle;
            listModel.ValueChangeCommand.Execute();
        };

        It should_create_list_with_40_answers = () =>
            listModel.Answers.Count.ShouldEqual(40);

        It should_add_item_with_Title_equals_trimmed_newListItemTitle = () =>
            listModel.Answers.Last().Title.ShouldEqual(newListItemTitle.Trim());

        It should_add_new_item_with_Value_equals_40 = () =>
            listModel.Answers.Last().Value.ShouldEqual(40m);

        It should_set_IsAddNewItemVisible_flag_in_false = () =>
            listModel.IsAddNewItemVisible.ShouldBeFalse();

        It should_clear_NewListItem_field = () =>
            listModel.NewListItem.ShouldBeEmpty();

        It should_send_answer_command = () =>
            AnsweringViewModelMock.Verify(x => x.SendAnswerQuestionCommandAsync(Moq.It.IsAny<AnswerTextListQuestionCommand>()), Times.Once);

        private static TextListQuestionViewModel listModel;
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
            new Tuple<decimal, string>(2m, "Answer 2"),
            new Tuple<decimal, string>(3m, "Answer 3"),
            new Tuple<decimal, string>(4m, "Answer 4"),
            new Tuple<decimal, string>(5m, "Answer 5"),
            new Tuple<decimal, string>(6m, "Answer 6"),
            new Tuple<decimal, string>(7m, "Answer 7"),
            new Tuple<decimal, string>(8m, "Answer 8"),
            new Tuple<decimal, string>(9m, "Answer 9"),
            new Tuple<decimal, string>(10m, "Answer 10"),
            new Tuple<decimal, string>(11m, "Answer 11"),
            new Tuple<decimal, string>(12m, "Answer 12"),
            new Tuple<decimal, string>(13m, "Answer 13"),
            new Tuple<decimal, string>(14m, "Answer 14"),
            new Tuple<decimal, string>(15m, "Answer 15"),
            new Tuple<decimal, string>(16m, "Answer 16"),
            new Tuple<decimal, string>(17m, "Answer 17"),
            new Tuple<decimal, string>(18m, "Answer 18"),
            new Tuple<decimal, string>(19m, "Answer 19"),
            new Tuple<decimal, string>(20m, "Answer 20"),
            new Tuple<decimal, string>(21m, "Answer 21"),
            new Tuple<decimal, string>(22m, "Answer 22"),
            new Tuple<decimal, string>(23m, "Answer 23"),
            new Tuple<decimal, string>(24m, "Answer 24"),
            new Tuple<decimal, string>(25m, "Answer 25"),
            new Tuple<decimal, string>(26m, "Answer 26"),
            new Tuple<decimal, string>(27m, "Answer 27"),
            new Tuple<decimal, string>(28m, "Answer 28"),
            new Tuple<decimal, string>(29m, "Answer 29"),
            new Tuple<decimal, string>(30m, "Answer 30"),
            new Tuple<decimal, string>(31m, "Answer 31"),
            new Tuple<decimal, string>(32m, "Answer 32"),
            new Tuple<decimal, string>(33m, "Answer 33"),
            new Tuple<decimal, string>(34m, "Answer 34"),
            new Tuple<decimal, string>(35m, "Answer 35"),
            new Tuple<decimal, string>(36m, "Answer 36"),
            new Tuple<decimal, string>(37m, "Answer 37"),
            new Tuple<decimal, string>(38m, "Answer 38"),
            new Tuple<decimal, string>(39m, "Answer 39"),
        };

        private static readonly string newListItemTitle = "   Hello World!      ";
    }
}