using System;
using System.Linq;
using System.Threading;
using MvvmCross.Tests;
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

        public CategoricalComboboxAutocompleteViewModelTests()
        {
            base.Setup();
            Ioc.RegisterType<ThrottlingViewModel>(() => Create.ViewModel.ThrottlingViewModel());
        }

        [Test]
        public void when_FilterCommand_then_AutoCompleteSuggestions_should_contains_filtered_options_only()
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
            vm.FilterCommand.Execute("2");
            Thread.Sleep(1000);
            // assert
            Assert.That(vm.AutoCompleteSuggestions.Count, Is.EqualTo(2));
            Assert.That(vm.AutoCompleteSuggestions.Select(x => x.Value), Is.EquivalentTo(new[] { 2, 3 }));
        }

        [Test]
        public void when_FilterCommand_and_has_excluded_options_then_AutoCompleteSuggestions_should_not_contains_excluded_options()
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
            vm.FilterCommand.Execute("1");
            Thread.Sleep(1000);
            // assert
            Assert.That(vm.AutoCompleteSuggestions.Count, Is.EqualTo(2));
            Assert.That(vm.AutoCompleteSuggestions.Select(x => x.Value), Is.EquivalentTo(new[] {2, 3}));
        }

        //[Test]
        //public void when_SaveAnswerBySelectedOptionCommand_then_OnAddOption_event_should_be_invoked_and_filter_should_be_dropped()
        //{
        //    // arrange
        //    var autocompleteQuestionId = Guid.Parse("11111111111111111111111111111111");
        //    var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
        //        Create.Entity.MultyOptionsQuestion(autocompleteQuestionId,
        //            new[]
        //            {
        //                Create.Entity.Answer("1", 1),
        //                Create.Entity.Answer("12", 2),
        //                Create.Entity.Answer("123", 3),
        //            }));

        //    var interview = SetUp.StatefulInterview(questionnaire);
        //    var filteredViewModel = Create.ViewModel.FilteredOptionsViewModel(Identity.Create(autocompleteQuestionId, RosterVector.Empty), questionnaire, interview);

        //    var mockOfOnAddEvent = new Mock<Func<object, int, Task>>();
        //    var vm = Create.ViewModel.CategoricalComboboxAutocompleteViewModel(filteredViewModel);
            
        //    vm.OnItemSelected += mockOfOnAddEvent.Object;
        //    vm.FilterCommand.Execute("2");

        //    // act
        //    vm.SaveAnswerBySelectedOptionCommand.Execute(Create.Entity.OptionWithSearchTerm(2));

        //    // assert
        //    mockOfOnAddEvent.Verify(x => x(vm, 2), Times.Once);
        //    Assert.That(vm.FilterText, Is.Null);
        //    Assert.That(vm.AutoCompleteSuggestions.Count, Is.EqualTo(3));
        //    Assert.That(vm.AutoCompleteSuggestions.Select(x => x.Value), Is.EquivalentTo(new[] {1, 2, 3}));
        //}
    }
}
