using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Moq;
using MvvmCross.Base;
using MvvmCross.Plugin.Messenger;
using MvvmCross.Tests;
using MvvmCross.Views;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.CascadingSingleOptionQuestionViewModelTests
{
    [TestOf(typeof(CategoricalComboboxAutocompleteViewModel))]
    internal class CategoricalComboboxAutocompleteViewModelTestContext : MvxIoCSupportingTest
    {
        public CategoricalComboboxAutocompleteViewModelTestContext()
        {
            base.Setup();

            var dispatcher = Create.Fake.MvxMainThreadDispatcher1();
            Ioc.RegisterSingleton<IMvxViewDispatcher>(dispatcher);
            Ioc.RegisterSingleton<IMvxMainThreadAsyncDispatcher>(dispatcher);
            Ioc.RegisterType<ThrottlingViewModel>(() => Create.ViewModel.ThrottlingViewModel());
        }

        protected static CategoricalComboboxAutocompleteViewModel CreateCategoricalComboboxAutocompleteViewModel(
            IQuestionStateViewModel questionState = null,
            FilteredOptionsViewModel filteredOptionsViewModel = null)
        {
            var cascadingSingleOptionQuestionViewModel = new CategoricalComboboxAutocompleteViewModel(
                questionState ?? Mock.Of<IQuestionStateViewModel>(),
                filteredOptionsViewModel ?? Mock.Of<FilteredOptionsViewModel>(),
            true);

            return cascadingSingleOptionQuestionViewModel;
        }

        protected static IQuestionnaireStorage SetupQuestionnaireRepositoryWithCascadingQuestion(IOptionsRepository optionsRepository = null)
        {
            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.GetRosterLevelForEntity(parentIdentity.Id) == 1
                && _.GetCascadingQuestionParentId(questionIdentity.Id) == parentIdentity.Id
                && _.GetOptionsForQuestion(Moq.It.IsAny<Guid>(), Moq.It.IsAny<int?>(), Moq.It.IsAny<string>(), It.IsAny<int[]>()) ==  Options
            );

            return Mock.Of<IQuestionnaireStorage>(x => x.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == questionnaire
            && x.GetQuestionnaireOrThrow(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == questionnaire);
        }

        protected static IOptionsRepository SetupOptionsRepositoryForQuestionnaire(Guid questionId, QuestionnaireIdentity questionnaireIdentity = null)
        {
            var optionsRepository = new Mock<IOptionsRepository>();
            return optionsRepository.Object;
        }

        protected static void SetUp()
        {
            navigationState = Create.Other.NavigationState();
            QuestionStateMock = new Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> { DefaultValue = DefaultValue.Mock };
            var userInterfaceStateService = Mock.Of<IUserInterfaceStateService>(
                x => x.WaitWhileUserInterfaceIsRefreshingAsync() == Task.FromResult(true));
            
            AnsweringViewModelMock = new Mock<AnsweringViewModel>(Mock.Of<ICommandService>(), userInterfaceStateService, Mock.Of<IMvxMessenger>());
            
            EventRegistry = new Mock<IViewModelEventRegistry>();
        }

        protected static Identity questionIdentity = Create.Entity.Identity(Guid.Parse("11111111111111111111111111111111"), new decimal[] { 1, 2 });

        protected static Identity parentIdentity = Create.Entity.Identity(Guid.Parse("22222222222222222222222222222222"), new decimal[] { 1 });

        protected static NavigationState navigationState;

        protected static Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> QuestionStateMock;

        protected static Mock<AnsweringViewModel> AnsweringViewModelMock;

        protected static Mock<IViewModelEventRegistry> EventRegistry;

        protected static ReadOnlyCollection<CategoricalOption> Options = new List<CategoricalOption>
        {
            Create.Entity.CategoricalQuestionOption(1, "title abc 1", 1),
            Create.Entity.CategoricalQuestionOption(2, "title def 2", 1),
            Create.Entity.CategoricalQuestionOption(3, "title klo 3", 1),
            Create.Entity.CategoricalQuestionOption(4, "title gha 4", 2),
            Create.Entity.CategoricalQuestionOption(5, "title ccc 5", 2),
            Create.Entity.CategoricalQuestionOption(6, "title bcw 6", 2)
        }.ToReadOnlyCollection();

        protected static readonly string interviewId = "Some interviewId";

        protected static Guid interviewGuid = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

        protected static readonly Guid userId = Guid.Parse("ffffffffffffffffffffffffffffffff");

        protected static readonly QuestionnaireIdentity questionnaireId =
            Create.Entity.QuestionnaireIdentity(Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"), 1);
    }
}
