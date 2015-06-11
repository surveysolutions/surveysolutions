using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.FilteredSingleOptionQuestionViewModelTests
{
    public class when_entering_filter_text_equals_to_ones_from_options : FilteredSingleOptionQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            interviewId = "interviewId";
            questionnaireId = "questionnaireId";
            userId = Guid.NewGuid();
            questionIdentity = Create.Identity(Guid.NewGuid(), new decimal[0]);

            var singleOptionAnswer = Mock.Of<SingleOptionAnswer>(_ => _.Answer == 3);

            var interview = Mock.Of<IStatefulInterview>(_
                => _.QuestionnaireId == questionnaireId
                   && _.GetSingleOptionAnswer(questionIdentity) == singleOptionAnswer);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(_ => _.Get(interviewId) == interview);

            var filteredSingleOptionQuestionModel = Mock.Of<FilteredSingleOptionQuestionModel>(_
                => _.Id == questionIdentity.Id
                   && _.Options == new List<OptionModel>()
                   {
                       new OptionModel() {Title = "abc", Value = 1},
                       new OptionModel() {Title = "bac", Value = 2},
                       new OptionModel() {Title = "bbc", Value = 3},
                       new OptionModel() {Title = "bba", Value = 4},
                       new OptionModel() {Title = "ccc", Value = 5},
                   });

            var questionnaireModel = Mock.Of<QuestionnaireModel>(_ => _.Questions == new Dictionary<Guid, BaseQuestionModel> { { questionIdentity.Id, filteredSingleOptionQuestionModel } });

            var questionnaireRepository = Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(x => x.GetById(questionnaireId) == questionnaireModel);

            var userIdentity = Mock.Of<IUserIdentity>(_ => _.UserId == userId);
            var principal = Mock.Of<IPrincipal>(_ => _.CurrentUserIdentity == userIdentity);

            questionStateMock = new Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> { DefaultValue = DefaultValue.Mock };
            answeringViewModelMock = new Mock<AnsweringViewModel>() { DefaultValue = DefaultValue.Mock };
            
            viewModel = CreateFilteredSingleOptionQuestionViewModel(
                questionStateViewModel: questionStateMock.Object,
                answering: answeringViewModelMock.Object,
                principal: principal,
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository);

            var navigationState = CreateNavigationState();
            viewModel.Init(interviewId, questionIdentity, navigationState);
        };

        private Because of = () =>
            viewModel.FilterText = "bba";

        It should_update_suggestions_list = () =>
            viewModel.AutoCompleteSuggestions.Count.ShouldEqual(1);

        It should_send_save_answer_command = () =>
            answeringViewModelMock.Verify(
                a => a.SendAnswerQuestionCommand(Moq.It.IsAny<AnswerQuestionCommand>()), 
                Times.AtLeastOnce);



        private static FilteredSingleOptionQuestionViewModel viewModel;
        private static Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> questionStateMock;
        private static Mock<AnsweringViewModel> answeringViewModelMock;
        private static Identity questionIdentity;
        private static string interviewId;
        private static string questionnaireId;
        private static Guid userId;
    }
}