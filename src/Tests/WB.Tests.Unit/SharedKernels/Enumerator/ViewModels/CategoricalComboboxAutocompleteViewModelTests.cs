using System;
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
            Thread.Sleep(1000);
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
            Thread.Sleep(1000);
            // assert
            Assert.That(vm.AutoCompleteSuggestions.Count, Is.EqualTo(2));
            Assert.That(vm.AutoCompleteSuggestions.Select(x => x.Value), Is.EquivalentTo(new[] {2, 3}));
        }
    }
}
