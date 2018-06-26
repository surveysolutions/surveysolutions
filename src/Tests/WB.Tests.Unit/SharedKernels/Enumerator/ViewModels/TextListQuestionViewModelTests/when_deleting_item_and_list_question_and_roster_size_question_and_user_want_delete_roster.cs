using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.TextListQuestionViewModelTests
{
    internal class when_deleting_item_and_list_question_and_roster_size_question_and_user_want_delete_roster : TextListQuestionViewModelTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var textListAnswer = Mock.Of<InterviewTreeTextListQuestion>(_ => _.GetAnswer() == savedAnswers && _.IsAnswered() == true);

            var interview = Mock.Of<IStatefulInterview>(_
                => _.QuestionnaireId == questionnaireId
                   && _.GetTextListQuestion(questionIdentity) == textListAnswer);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(_ => _.Get(interviewId) == interview);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithListQuestion();

            var userIdentity = Mock.Of<IUserIdentity>(_ => _.UserId == userId);
            var principal = Mock.Of<IPrincipal>(_ => _.CurrentUserIdentity == userIdentity && _.IsAuthenticated == true);

            var userInteraction = new Mock<IUserInteractionService>();

            userInteraction
                .Setup(x => x.ConfirmAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<bool>()))
                .ReturnsAsync(true);

            listModel = CreateTextListQuestionViewModel(
                QuestionStateMock.Object,
                AnsweringViewModelMock.Object,
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository,
                principal: principal,
                userInteractionService: userInteraction.Object);

            listModel.Init(interviewId, questionIdentity, navigationState);
            BecauseOf();
        }

        public void BecauseOf() =>
            answerViewModels[deletedItemIndex].DeleteListItemCommand.Execute();

        [NUnit.Framework.Test] public void should_create_list_with_4_answers () =>
            answerViewModels.Count.Should().Be(4);

        [NUnit.Framework.Test] public void should_delete_item_with_index_equals__deletedItemIndex__ () =>
            answerViewModels.Any(x
                => x.Value == savedAnswers.ToTupleArray()[deletedItemIndex].Item1
                   && x.Title == savedAnswers.ToTupleArray()[deletedItemIndex].Item2)
                .Should().BeFalse();

        [NUnit.Framework.Test] public void should_contain_add_new_item_view_model () =>
            listModel.Answers.OfType<TextListAddNewItemViewModel>().Should().NotBeEmpty();

        [NUnit.Framework.Test] public void should_send_answer_command () =>
            AnsweringViewModelMock.Verify(x => x.SendAnswerQuestionCommandAsync(Moq.It.IsAny<AnswerTextListQuestionCommand>()), Times.Once);

        private static TextListQuestionViewModel listModel;
        private static NavigationState navigationState = Create.Other.NavigationState();

        private static readonly Mock<QuestionStateViewModel<TextListQuestionAnswered>> QuestionStateMock =
           new Mock<QuestionStateViewModel<TextListQuestionAnswered>> { DefaultValue = DefaultValue.Mock };

        private static readonly Mock<AnsweringViewModel> AnsweringViewModelMock =
            new Mock<AnsweringViewModel>() { DefaultValue = DefaultValue.Mock };

        private static readonly string interviewId = "44444444444444444444444444444444";

        private static readonly string questionnaireId = "Questionnaire Id";
        private static readonly Guid userId = Guid.Parse("ffffffffffffffffffffffffffffffff");

        private static readonly TextListAnswer savedAnswers = TextListAnswer.FromTupleArray(new[]
        {
            new Tuple<decimal, string>(1m, "Answer 1"),
            new Tuple<decimal, string>(3m, "Answer 3"),
            new Tuple<decimal, string>(4m, "Answer 5"),
            new Tuple<decimal, string>(8m, "Answer 8"),
            new Tuple<decimal, string>(9m, "Answer 9"),
        });

        private static readonly int deletedItemIndex = 2;
        private static List<TextListItemViewModel> answerViewModels => listModel.Answers.OfType<TextListItemViewModel>().ToList();
    }
}
