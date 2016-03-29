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
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.TextListQuestionViewModelTests
{
    internal class when_deleting_item_and_list_question_and_roster_size_question_and_user_want_delete_roster : TextListQuestionViewModelTestContext
    {
        Establish context = () =>
        {
            var textListAnswer = Mock.Of<TextListAnswer>(_ => _.Answers == savedAnswers && _.IsAnswered == true);

            var interview = Mock.Of<IStatefulInterview>(_
                => _.QuestionnaireId == questionnaireId
                   && _.GetTextListAnswer(questionIdentity) == textListAnswer);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(_ => _.Get(interviewId) == interview);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithListQuestion();

            var userIdentity = Mock.Of<IUserIdentity>(_ => _.UserId == userId);
            var principal = Mock.Of<IPrincipal>(_ => _.CurrentUserIdentity == userIdentity);

            var userInteraction = new Mock<IUserInteractionService>();

            userInteraction
                .Setup(x => x.ConfirmAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .ReturnsAsync(true);

            listModel = CreateTextListQuestionViewModel(
                QuestionStateMock.Object,
                AnsweringViewModelMock.Object,
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository,
                principal: principal,
                userInteractionService: userInteraction.Object);

            listModel.InitAsync(interviewId, questionIdentity, navigationState).WaitAndUnwrapException();
        };

        Because of = () =>
            listModel.Answers[deletedItemIndex].DeleteListItemCommand.Execute();

        It should_create_list_with_4_answers = () =>
            listModel.Answers.Count.ShouldEqual(4);

        It should_delete_item_with_index_equals__deletedItemIndex__ = () =>
            listModel.Answers.Any(x
                => x.Value == savedAnswers[deletedItemIndex].Item1
                   && x.Title == savedAnswers[deletedItemIndex].Item2)
                .ShouldBeFalse();

        It should_set_IsAddNewItemVisible_flag_in_true = () =>
            listModel.IsAddNewItemVisible.ShouldBeTrue();

        It should_send_answer_command = () =>
            AnsweringViewModelMock.Verify(x => x.SendAnswerQuestionCommandAsync(Moq.It.IsAny<AnswerTextListQuestionCommand>()), Times.Once);

        private static TextListQuestionViewModel listModel;
        private static NavigationState navigationState = Create.NavigationState();

        private static readonly Mock<QuestionStateViewModel<TextListQuestionAnswered>> QuestionStateMock =
           new Mock<QuestionStateViewModel<TextListQuestionAnswered>> { DefaultValue = DefaultValue.Mock };

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
                                                                            new Tuple<decimal, string>(8m, "Answer 8"),
                                                                            new Tuple<decimal, string>(9m, "Answer 9"),
                                                                        };

        private static readonly int deletedItemIndex = 2;
    }
}