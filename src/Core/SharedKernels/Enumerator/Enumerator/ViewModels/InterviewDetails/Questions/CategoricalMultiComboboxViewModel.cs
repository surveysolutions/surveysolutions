using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class CategoricalMultiComboboxViewModel : CategoricalMultiViewModel
    {
        private string interviewId;
        private readonly CategoricalComboboxAutocompleteViewModel comboboxViewModel;
        private readonly CovariantObservableCollection<ICompositeEntity> comboboxCollection = new CovariantObservableCollection<ICompositeEntity>();

        public CategoricalMultiComboboxViewModel(
            QuestionStateViewModel<MultipleOptionsQuestionAnswered> questionStateViewModel,
            IQuestionnaireStorage questionnaireRepository, ILiteEventRegistry eventRegistry,
            IStatefulInterviewRepository interviewRepository, IPrincipal principal,
            IUserInteractionService userInteraction, AnsweringViewModel answering,
            FilteredOptionsViewModel filteredOptionsViewModel, QuestionInstructionViewModel instructionViewModel,
            ThrottlingViewModel throttlingModel) : base(questionStateViewModel, questionnaireRepository, eventRegistry,
            interviewRepository, principal, userInteraction, answering, filteredOptionsViewModel, instructionViewModel,
            throttlingModel)
        {
            this.comboboxViewModel =
                new CategoricalComboboxAutocompleteViewModel(questionStateViewModel, filteredOptionsViewModel, false);
        }

        public override void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.interviewId = interviewId;
            base.Init(interviewId, entityIdentity, navigationState);

            this.comboboxViewModel.Init(interviewId, entityIdentity, navigationState);
            this.comboboxViewModel.OnItemSelected += ComboboxInstantViewModel_OnItemSelected;
        }

        protected override void Init(IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            base.Init(interview, questionnaire);

            var answeredOptions = this.GetAnsweredOptionsFromInterview(interview);

            this.UpdateComboboxInMainThread(answeredOptions);
        }

        private void UpdateComboboxInMainThread(int[] answeredOptions)
        {
            this.InvokeOnMainThread(() =>
            {
                answeredOptions = answeredOptions ?? Array.Empty<int>();

                this.comboboxViewModel.ExcludeOptions(answeredOptions);

                var hasNoOptionsForAnswers = answeredOptions.Length == this.maxAllowedAnswers ||
                                             answeredOptions.Length == this.filteredOptionsViewModel.GetOptions().Count;

                if (hasNoOptionsForAnswers && this.comboboxCollection.Contains(this.comboboxViewModel))
                    this.comboboxCollection.Remove(this.comboboxViewModel);

                else if (!hasNoOptionsForAnswers && !this.comboboxCollection.Contains(this.comboboxViewModel))
                    this.comboboxCollection.Add(this.comboboxViewModel);

                comboboxViewModel.UpdateFilter(comboboxViewModel.FilterText);
            });
        }

        private async void ComboboxInstantViewModel_OnItemSelected(object sender, int selectedOptionCode)
        {
            var interview = this.interviewRepository.Get(this.interviewId);

            this.selectedOptionsToSave = (this.GetAnsweredOptionsFromInterview(interview) ?? Array.Empty<int>()).Union(new[] { selectedOptionCode }).ToArray();
            await this.throttlingModel.ExecuteActionIfNeeded();
        }

        protected override void AddCustomViewModels(CompositeCollection<ICompositeEntity> compositeCollection) 
            => compositeCollection.AddCollection(this.comboboxCollection);

        protected override void UpdateBorders() { }

        protected override IEnumerable<CategoricalMultiOptionViewModel<int>> GetOptions(IStatefulInterview interview)
        {
            var answeredOptions = this.GetAnsweredOptionsFromInterview(interview);
            if(answeredOptions == null) yield break;

            var allOptions = this.filteredOptionsViewModel.GetOptions();

            foreach (var optionCode in answeredOptions)
            {
                var categoricalOption = allOptions.Find(x => x.Value == optionCode);

                var vm = new CategoricalMultiComboboxOptionViewModel(this.userInteraction);

                base.InitViewModel(categoricalOption.Title, categoricalOption.Value, interview, vm,
                    interview.IsAnswerProtected(this.Identity, categoricalOption.Value));

                if (this.isRosterSizeQuestion) vm.MakeRosterSize();

                yield return vm;
            }
        }

        public override void Handle(AnswersRemoved @event)
        {
            if (!@event.Questions.Any(x => x.Id == this.Identity.Id && x.RosterVector.Identical(this.Identity.RosterVector))) return;

            this.UpdateComboboxInMainThread(null);
            this.UpdateViewModelsInMainThread();
        }

        public override void Handle(MultipleOptionsQuestionAnswered @event)
        {
            if (@event.QuestionId != this.Identity.Id || !@event.RosterVector.Identical(this.Identity.RosterVector)) return;

            this.UpdateComboboxInMainThread(@event.SelectedValues.Select(Convert.ToInt32).ToArray());
            this.UpdateViewModelsInMainThread();
        }

        public override void Dispose()
        {
            this.comboboxViewModel.OnItemSelected -= this.ComboboxInstantViewModel_OnItemSelected;
            this.comboboxViewModel.Dispose();

            base.Dispose();
        }
    }
}
