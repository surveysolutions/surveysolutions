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
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.TextListQuestionViewModelTests
{
    internal class when_adding_new_item_in_list_view_model_and_there_is_answer_in_interview : TextListQuestionViewModelTestContext
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

            listModel = CreateTextListQuestionViewModel(
                QuestionStateMock.Object,
                AnsweringViewModelMock.Object,
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository,
                principal: principal);

            listModel.Init(interviewId, questionIdentity, navigationState);
            BecauseOf();
        }

        public void BecauseOf()
        {
            var textListAddNewItemViewModel = listModel.Answers.OfType<TextListAddNewItemViewModel>().FirstOrDefault();

            textListAddNewItemViewModel.Text = newListItemTitle;
            textListAddNewItemViewModel.AddNewItemCommand.Execute();
        }

        [NUnit.Framework.Test] public void should_create_list_with_5_answers () =>
            answerViewModels.Count.Should().Be(5);

        [NUnit.Framework.Test] public void should_add_item_with_Title_equals_trimmed_newListItemTitle () =>
            answerViewModels.Last().Title.Should().Be(newListItemTitle.Trim());

        [NUnit.Framework.Test] public void should_add_new_item_with_Value_equals_9 () =>
            answerViewModels.Last().Value.Should().Be(9m);

        [NUnit.Framework.Test] public void should_not_contain_add_new_item_view_model () =>
            listModel.Answers.OfType<TextListAddNewItemViewModel>().Should().BeEmpty();

        [NUnit.Framework.Test] public void should_send_answer_command () =>
            AnsweringViewModelMock.Verify(x => x.SendAnswerQuestionCommandAsync(Moq.It.IsAny<AnswerTextListQuestionCommand>()), Times.Once);

        private static TextListQuestionViewModel listModel;
        private static readonly NavigationState navigationState = Create.Other.NavigationState();

        private static readonly Mock<QuestionStateViewModel<TextListQuestionAnswered>> QuestionStateMock = 
            new Mock<QuestionStateViewModel<TextListQuestionAnswered>>{ DefaultValue = DefaultValue.Mock };

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
            new Tuple<decimal, string>(8m, "Answer 8")
        });

        private static readonly string newListItemTitle = "   Hello World!      ";
        private static List<TextListItemViewModel> answerViewModels => listModel.Answers.OfType<TextListItemViewModel>().ToList();
    }
}
