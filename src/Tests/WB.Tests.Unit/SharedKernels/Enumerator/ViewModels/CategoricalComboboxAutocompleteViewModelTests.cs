using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using MvvmCross.Base;
using MvvmCross.Plugin.Messenger;
using MvvmCross.Tests;
using MvvmCross.Views;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels
{
    [TestOf(typeof(CategoricalComboboxAutocompleteViewModel))]
    public class CategoricalComboboxAutocompleteViewModelTests : MvxIoCSupportingTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            base.Setup();
            
            var dispatcher = Create.Fake.MvxMainThreadDispatcher1();
            Ioc.RegisterSingleton<IMvxViewDispatcher>(dispatcher);
            Ioc.RegisterSingleton<IMvxMainThreadAsyncDispatcher>(dispatcher);
            Ioc.RegisterType<ThrottlingViewModel>(() => Create.ViewModel.ThrottlingViewModel());
            Ioc.RegisterSingleton<IMvxMessenger>(Mock.Of<IMvxMessenger>());
        }

        [Test]
        public async Task when_FilterCommand_then_AutoCompleteSuggestions_should_contains_filtered_options_only()
        {
            // arrange
            var autocompleteQuestionId = Guid.Parse("11111111111111111111111111111111");
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultyOptionsQuestion(autocompleteQuestionId,
                    new[]
                    {
                        Create.Entity.Answer("1", 1),
                        Create.Entity.Answer("12", 2),
                        Create.Entity.Answer("123", 3),
                    }));

            var interview = SetUp.StatefulInterview(questionnaire);
            var excludedOptions = new[] { 1 };
            var filteredViewModel = Create.ViewModel.FilteredOptionsViewModel(Identity.Create(autocompleteQuestionId, RosterVector.Empty), questionnaire, interview);
            var vm = Create.ViewModel.CategoricalComboboxAutocompleteViewModel(filteredViewModel);
            // act
            await vm.FilterCommand.ExecuteAsync("2");
            // assert
            Assert.That(vm.AutoCompleteSuggestions.Count, Is.EqualTo(2));
            Assert.That(vm.AutoCompleteSuggestions.Select(x => x.Value), Is.EquivalentTo(new[] { 2, 3 }));
        }

        [Test]
        public async Task when_FilterCommand_and_has_excluded_options_then_AutoCompleteSuggestions_should_not_contains_excluded_options()
        {
            // arrange
            var autocompleteQuestionId = Guid.Parse("11111111111111111111111111111111");
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultyOptionsQuestion(autocompleteQuestionId,
                    new[]
                    {
                        Create.Entity.Answer("1", 1),
                        Create.Entity.Answer("12", 2),
                        Create.Entity.Answer("123", 3),
                    }));

            var interview = SetUp.StatefulInterview(questionnaire);
            var excludedOptions = new[] {1};
            var filteredViewModel = Create.ViewModel.FilteredOptionsViewModel(Identity.Create(autocompleteQuestionId, RosterVector.Empty), questionnaire, interview);
            var vm = Create.ViewModel.CategoricalComboboxAutocompleteViewModel(filteredViewModel);
            vm.ExcludeOptions(excludedOptions);
            // act
            await vm.FilterCommand.ExecuteAsync("1");
            // assert
            Assert.That(vm.AutoCompleteSuggestions.Count, Is.EqualTo(2));
            Assert.That(vm.AutoCompleteSuggestions.Select(x => x.Value), Is.EquivalentTo(new[] {2, 3}));
        }

        [Test]
        public async Task when_FilterCommand_then_should_not_toggle_Loading_state()
        {
            // arrange
            var autocompleteQuestionId = Guid.Parse("11111111111111111111111111111111");
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultyOptionsQuestion(autocompleteQuestionId,
                    new[]
                    {
                        Create.Entity.Answer("1", 1),
                        Create.Entity.Answer("12", 2),
                        Create.Entity.Answer("123", 3),
                    }));

            var interview = SetUp.StatefulInterview(questionnaire);
            var filteredViewModel = Create.ViewModel.FilteredOptionsViewModel(Identity.Create(autocompleteQuestionId, RosterVector.Empty), questionnaire, interview);
            var vm = Create.ViewModel.CategoricalComboboxAutocompleteViewModel(filteredViewModel);

            var changedProperties = new List<string>();
            vm.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);

            // act
            await vm.FilterCommand.ExecuteAsync(string.Empty);

            // assert
            Assert.That(changedProperties, Does.Not.Contain(nameof(CategoricalComboboxAutocompleteViewModel.Loading)));
            Assert.That(vm.Loading, Is.False);
        }

        [Test]
        public async Task when_FilterCommand_and_options_loading_is_slow_then_should_toggle_Loading_state()
        {
            // arrange
            var filteredViewModel = new Mock<FilteredOptionsViewModel>();
            filteredViewModel.Setup(x => x.GetOptions(It.IsAny<string>(), It.IsAny<int[]>(), It.IsAny<int?>()))
                .Returns(() =>
                {
                    Thread.Sleep(350);
                    return new List<CategoricalOption>
                    {
                        Create.Entity.CategoricalQuestionOption(1, "option 1", null),
                    };
                });

            var vm = Create.ViewModel.CategoricalComboboxAutocompleteViewModel(filteredViewModel.Object);
            var loadingStates = new List<bool>();
            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(CategoricalComboboxAutocompleteViewModel.Loading))
                    loadingStates.Add(vm.Loading);
            };

            // act
            await vm.FilterCommand.ExecuteAsync("o");

            // assert
            Assert.That(loadingStates, Does.Contain(true));
            Assert.That(vm.Loading, Is.False);
        }
    }
}
