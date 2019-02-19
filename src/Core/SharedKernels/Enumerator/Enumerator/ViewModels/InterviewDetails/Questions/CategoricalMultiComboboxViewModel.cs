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
        private readonly CategoricalMultiComboboxAutocompleteViewModel comboboxViewModel;
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
                new CategoricalMultiComboboxAutocompleteViewModel(questionStateViewModel, filteredOptionsViewModel);
        }

        public override void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.interviewId = interviewId;
            base.Init(interviewId, entityIdentity, navigationState);

            this.comboboxViewModel.Init(interviewId, entityIdentity, navigationState);
            this.comboboxViewModel.OnAddOption += ComboboxInstantViewModel_OnAddOption;
        }

        protected override void Init(IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            base.Init(interview, questionnaire);

            var answeredOptions = this.GetAnsweredOptionsFromInterview(interview);

            this.UpdateCombobox(answeredOptions);
        }

        private void UpdateCombobox(int[] answeredOptions)
        {
            answeredOptions = answeredOptions ?? Array.Empty<int>();

            this.comboboxViewModel.ExcludeOptions(answeredOptions);

            var hasNoOptionsForAnswers = answeredOptions.Length == this.maxAllowedAnswers ||
                                         answeredOptions.Length == this.filteredOptionsViewModel.GetOptions().Count;

            if (hasNoOptionsForAnswers && this.comboboxCollection.Contains(this.comboboxViewModel))
                this.comboboxCollection.Remove(this.comboboxViewModel);

            else if (!hasNoOptionsForAnswers && !this.comboboxCollection.Contains(this.comboboxViewModel))
                this.comboboxCollection.Add(this.comboboxViewModel);
        }

        private async void ComboboxInstantViewModel_OnAddOption(object sender, int selectedOptionCode)
        {
            var interview = this.interviewRepository.Get(this.interviewId);

            this.selectedOptionsToSave = (this.GetAnsweredOptionsFromInterview(interview) ?? Array.Empty<int>()).Union(new[] { selectedOptionCode }).ToArray();
            await this.throttlingModel.ExecuteActionIfNeeded();
        }

        public override IObservableCollection<ICompositeEntity> Children
        {
            get
            {
                var result = new CompositeCollection<ICompositeEntity>();
                result.Add(new OptionBorderViewModel(this.QuestionState, true));
                result.AddCollection(this.Options);
                result.AddCollection(this.comboboxCollection);
                result.Add(new OptionBorderViewModel(this.QuestionState, false));
                return result;
            }
        }

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

            this.InvokeOnMainThread(() =>
            {
                this.UpdateCombobox(null);
                this.UpdateViewModels();
            });
        }

        public override void Handle(MultipleOptionsQuestionAnswered @event)
        {
            if (@event.QuestionId != this.Identity.Id || !@event.RosterVector.Identical(this.Identity.RosterVector)) return;

            this.InvokeOnMainThread(() =>
            {
                this.UpdateCombobox(@event.SelectedValues.Select(Convert.ToInt32).ToArray());
                this.UpdateViewModels();
            });
        }

        public override void Dispose()
        {
            this.comboboxViewModel.OnAddOption -= this.ComboboxInstantViewModel_OnAddOption;
            this.comboboxViewModel.Dispose();

            base.Dispose();
        }
    }
}
