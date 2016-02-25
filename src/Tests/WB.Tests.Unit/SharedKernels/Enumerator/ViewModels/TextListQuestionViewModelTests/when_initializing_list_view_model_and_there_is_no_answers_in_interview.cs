using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.TextListQuestionViewModelTests
{
    internal class when_initializing_list_view_model_and_there_is_no_answers_in_interview : TextListQuestionViewModelTestContext
    {
        Establish context = () =>
        {
            var textListAnswer = Mock.Of<TextListAnswer>(_ => _.IsAnswered == false);

            var interview = Mock.Of<IStatefulInterview>(_ => _.QuestionnaireId == questionnaireId
                && _.GetTextListAnswer(questionIdentity) == textListAnswer);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == interview);

            var textListQuestionModel = Mock.Of<TextListQuestionModel>(_ 
                => _.Id == questionIdentity.Id
                   && _.IsRosterSizeQuestion == true
                   && _.MaxAnswerCount == 5);

            var questionnaireModel = Mock.Of<QuestionnaireModel>(_ => _.Questions == new Dictionary<Guid, BaseQuestionModel>{ { questionIdentity.Id, textListQuestionModel } });

            var questionnaireIdentity = Create.QuestionnaireIdentity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"));
            var questionnaireRepository = Unit.Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireIdentity,
                _ => _.GetQuestionTitle(questionIdentity.Id) == "Title");

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

        It should_create_empty_list_of_answers = () => 
            listModel.Answers.Count.ShouldEqual(0);

        It should_set_IsAddNewItemVisible_flag_in_true = () =>
            listModel.IsAddNewItemVisible.ShouldBeTrue();

        private static TextListQuestionViewModel listModel;
        private static Identity questionIdentity = Create.Identity(Guid.Parse("11111111111111111111111111111111"), new decimal[0]);
        private static NavigationState navigationState = Create.NavigationState();
        private static readonly Mock<QuestionStateViewModel<TextListQuestionAnswered>> QuestionStateMock = new Mock<QuestionStateViewModel<TextListQuestionAnswered>>();
        private static readonly Mock<AnsweringViewModel> AnsweringViewModelMock = new Mock<AnsweringViewModel>();

        private static readonly string interviewId = "Some interviewId";
        private static readonly string questionnaireId = "Questionnaire Id";
    }
}