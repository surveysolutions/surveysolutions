using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Tester.Implementation.Aggregates;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.BoundedContexts.Tester.ViewModels.Questions.State;
using WB.Core.BoundedContexts.Tester.ViewModels.Questions;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Tester.TextListQuestionViewModelTests
{
    public class when_initializing_list_view_model_and_there_is_no_answers_in_interview : TextListQuestionViewModelTestContext
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