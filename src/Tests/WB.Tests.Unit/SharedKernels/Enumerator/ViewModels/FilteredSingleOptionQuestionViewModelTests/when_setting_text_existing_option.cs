﻿using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using It = Machine.Specifications.It;
using System.Threading;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.FilteredSingleOptionQuestionViewModelTests
{
    internal class when_setting_text_existing_option : FilteredSingleOptionQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            Stub.InitMvxMainThreadDispatcher();

            var singleOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.GetAnswer() == Create.Entity.SingleOptionAnswer(3));
            var option = new CategoricalOption() {Value = 1, Title = "dfdf" + answerValue };

            var interview = Mock.Of<IStatefulInterview>(_
                => _.QuestionnaireIdentity == questionnaireId
                   && _.GetSingleOptionQuestion(questionIdentity) == singleOptionAnswer &&
                   _.GetTopFilteredOptionsForQuestion(questionIdentity, null, answerValue, 200) == new List<CategoricalOption> () { option});

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(_ => _.Get(interviewId) == interview);

            var userIdentity = Mock.Of<IUserIdentity>(_ => _.UserId == userId);
            var principal = Mock.Of<IPrincipal>(_ => _.CurrentUserIdentity == userIdentity);

            questionStateMock = new Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> { DefaultValue = DefaultValue.Mock };
            var answerViewModel = new AnsweringViewModel(Mock.Of<ICommandService>(), Mock.Of<IUserInterfaceStateService>());

            var filteredOptionsViewModel = Setup.FilteredOptionsViewModel();

            viewModel = CreateFilteredSingleOptionQuestionViewModel(
                questionStateViewModel: questionStateMock.Object,
                answering: answerViewModel,
                principal: principal,
                interviewRepository: interviewRepository,
                filteredOptionsViewModel: filteredOptionsViewModel);
            viewModel.DefaultText = string.Empty;

            var navigationState = Create.Other.NavigationState();
            viewModel.Init(interviewId, questionIdentity, navigationState);
        };

        Because of = () => {
            viewModel.FilterText = answerValue;
            Thread.Sleep(1000);
        };

        It should_set_value = () =>
            viewModel.FilterText.ShouldEqual(answerValue);

        It should_provide_suggesions = () =>
            viewModel.AutoCompleteSuggestions.Count.ShouldEqual(1);
        

        private static FilteredSingleOptionQuestionViewModel viewModel;
        private static Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> questionStateMock;
        private static string interviewId = "interviewId";
        private static readonly Guid userId = Guid.NewGuid();

        private static string answerValue = "é";
    }
}