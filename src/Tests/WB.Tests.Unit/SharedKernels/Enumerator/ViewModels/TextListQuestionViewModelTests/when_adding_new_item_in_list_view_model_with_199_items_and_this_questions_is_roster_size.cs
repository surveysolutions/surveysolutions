using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.Enumerator.Aggregates;

using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.TextListQuestionViewModelTests
{
    internal class when_adding_new_item_in_list_view_model_with_199_items_and_this_questions_is_roster_size : TextListQuestionViewModelTestContext
    {
        Establish context = () =>
        {
            var textListAnswer = Mock.Of<InterviewTreeTextListQuestion>(_ => _.GetAnswer().ToTupleArray() == savedAnswers && _.IsAnswered == true);

            var interview = Mock.Of<IStatefulInterview>(_
                => _.QuestionnaireId == questionnaireId
                   && _.GetTextListQuestion(questionIdentity) == textListAnswer);

            for (int i = 1; i <= 199; i++)
            {
                savedAnswers[i-1] = new Tuple<decimal, string>(i, $"Answer {i}");
            }

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

            listModel.Init(interviewId, questionIdentity, navigationState);
        };

        Because of = () =>
        {
            var textListAddNewItemViewModel = listModel.Answers.OfType<TextListAddNewItemViewModel>().FirstOrDefault();

            textListAddNewItemViewModel.Text = newListItemTitle;
            textListAddNewItemViewModel.AddNewItemCommand.Execute();
        };

        It should_create_list_with_200_answers = () =>
            answerViewModels.Count.ShouldEqual(200);

        It should_add_item_with_Title_equals_trimmed_newListItemTitle = () =>
            answerViewModels.Last().Title.ShouldEqual(newListItemTitle.Trim());

        It should_add_new_item_with_Value_equals_200 = () =>
            answerViewModels.Last().Value.ShouldEqual(200m);

        It should_not_contain_add_new_item_view_model = () =>
            listModel.Answers.OfType<TextListAddNewItemViewModel>().ShouldBeEmpty();

        It should_send_answer_command = () =>
            AnsweringViewModelMock.Verify(x => x.SendAnswerQuestionCommandAsync(Moq.It.IsAny<AnswerTextListQuestionCommand>()), Times.Once);

        private static TextListQuestionViewModel listModel;
        private static readonly NavigationState navigationState = Create.Other.NavigationState();

        private static readonly Mock<QuestionStateViewModel<TextListQuestionAnswered>> QuestionStateMock = 
            new Mock<QuestionStateViewModel<TextListQuestionAnswered>>{ DefaultValue = DefaultValue.Mock };

        private static readonly Mock<AnsweringViewModel> AnsweringViewModelMock = 
            new Mock<AnsweringViewModel> { DefaultValue = DefaultValue.Mock };

        private static readonly string interviewId = "44444444444444444444444444444444";

        private static readonly string questionnaireId = "Questionnaire Id";
        private static readonly Guid userId = Guid.Parse("ffffffffffffffffffffffffffffffff");

        private static readonly Tuple<decimal, string>[] savedAnswers = new Tuple<decimal, string>[199];

        private static readonly string newListItemTitle = "   Hello World!      ";
        private static List<TextListItemViewModel> answerViewModels => listModel.Answers.OfType<TextListItemViewModel>().ToList();
    }
}