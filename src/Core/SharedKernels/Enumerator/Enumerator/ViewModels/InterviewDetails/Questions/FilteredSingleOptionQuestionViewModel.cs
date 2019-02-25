using System;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class FilteredSingleOptionQuestionViewModel : BaseComboboxQuestionViewModel
    {

        public FilteredSingleOptionQuestionViewModel(
            IStatefulInterviewRepository interviewRepository,
            ILiteEventRegistry eventRegistry,
            FilteredOptionsViewModel filteredOptionsViewModel,
            IPrincipal principal,
            QuestionStateViewModel<SingleOptionQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering,
            QuestionInstructionViewModel instructionViewModel) :
            base(principal: principal, questionStateViewModel: questionStateViewModel, answering: answering,
                instructionViewModel: instructionViewModel, interviewRepository: interviewRepository, eventRegistry: eventRegistry, filteredOptionsViewModel)
        {
            this.comboboxViewModel = new CategoricalComboboxAutocompleteViewModel(questionStateViewModel, filteredOptionsViewModel, true);
        }

        protected override void Initialize(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.filteredOptionsViewModel.Init(interviewId, entityIdentity, SuggestionsMaxCount);
            this.filteredOptionsViewModel.OptionsChanged += FilteredOptionsViewModelOnOptionsChanged;

            this.comboboxViewModel.Init(interviewId, entityIdentity, navigationState);
            this.comboboxViewModel.OnItemSelected += ComboboxInstantViewModel_OnItemSelected;
            this.comboboxViewModel.OnAnswerRemoved += ComboboxInstantViewModel_OnAnswerRemoved;

            comboboxCollection.Add(comboboxViewModel);
        }

        private async void FilteredOptionsViewModelOnOptionsChanged(object sender, EventArgs eventArgs) => await comboboxViewModel.UpdateFilter(FilterText);

        
        public override void Dispose()
        {
            base.Dispose();
            this.filteredOptionsViewModel.OptionsChanged -= FilteredOptionsViewModelOnOptionsChanged;
            this.filteredOptionsViewModel.Dispose();

            this.comboboxViewModel.OnItemSelected -= this.ComboboxInstantViewModel_OnItemSelected;
            this.comboboxViewModel.OnAnswerRemoved -= ComboboxInstantViewModel_OnAnswerRemoved;
            this.comboboxViewModel.Dispose();
        }
        
        public override IObservableCollection<ICompositeEntity> Children
        {
            get
            {
                var result = new CompositeCollection<ICompositeEntity>();
                
                result.Add(this.optionsTopBorderViewModel);
                result.AddCollection(comboboxCollection);
                result.Add(this.optionsBottomBorderViewModel);

                return result;
            }
        }
    }
}
