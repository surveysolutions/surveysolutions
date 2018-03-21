using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.TextListQuestionViewModelTests
{
    internal class when_initializing_list_view_model_and_there_is_answer_in_interview : TextListQuestionViewModelTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var textListAnswer = Mock.Of<InterviewTreeTextListQuestion>(_ => _.GetAnswer() == savedAnswers && _.IsAnswered() == true);
            
            var interview = Mock.Of<IStatefulInterview>(_ 
                => _.QuestionnaireId == questionnaireId
                   && _.GetTextListQuestion(questionIdentity) == textListAnswer);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(_ => _.Get(interviewId) == interview);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithListQuestion(isRosterSizeQuestion: true);

            listModel = CreateTextListQuestionViewModel(
                QuestionStateMock.Object,
                AnsweringViewModelMock.Object,
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository);
            BecauseOf();
        }

        public void BecauseOf() =>
            listModel.Init(interviewId, questionIdentity, navigationState);

        [NUnit.Framework.Test] public void should_initialize_question_state () =>
            QuestionStateMock.Verify(x => x.Init(interviewId, questionIdentity, navigationState), Times.Once);

        [NUnit.Framework.Test] public void should_create_list_with_5_answers () =>
            answerViewModels.Count.Should().Be(5);

        [NUnit.Framework.Test] public void should_create_list_with_Values_same_as_in_saved_answers () =>
            answerViewModels.Select(x => x.Value).Should().BeEquivalentTo(savedAnswers.ToTupleArray().Select(x=> x.Item1));

        [NUnit.Framework.Test] public void should_create_list_with_Titles_same_as_in_saved_answers () =>
            answerViewModels.Select(x => x.Title).Should().BeEquivalentTo(savedAnswers.ToTupleArray().Select(x => x.Item2));

        [NUnit.Framework.Test] public void should_not_contain_add_new_item_view_model () =>
            listModel.Answers.OfType<TextListAddNewItemViewModel>().Should().BeEmpty();

        private static TextListQuestionViewModel listModel;
        private static NavigationState navigationState = Create.Other.NavigationState();
        private static readonly Mock<QuestionStateViewModel<TextListQuestionAnswered>> QuestionStateMock = new Mock<QuestionStateViewModel<TextListQuestionAnswered>>();
        private static readonly Mock<AnsweringViewModel> AnsweringViewModelMock = new Mock<AnsweringViewModel>();

        private static readonly string interviewId = "Some interviewId";
        private static readonly string questionnaireId = "Questionnaire Id";

        private static readonly TextListAnswer savedAnswers = TextListAnswer.FromTupleArray(new[]
        {
            new Tuple<decimal, string>(1m, "Answer 1"),
            new Tuple<decimal, string>(3m, "Answer 3"),
            new Tuple<decimal, string>(4m, "Answer 5"),
            new Tuple<decimal, string>(8m, "Answer 8"),
            new Tuple<decimal, string>(9m, "Answer 9"),
        });
        private static List<TextListItemViewModel> answerViewModels => listModel.Answers.OfType<TextListItemViewModel>().ToList();
    }
}
