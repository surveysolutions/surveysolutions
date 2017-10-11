using System;
using System.Collections.Generic;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class FilteredSingleOptionQuestionViewModel : BaseFilteredQuestionViewModel
    {
        private readonly FilteredOptionsViewModel filteredOptionsViewModel;

        public FilteredSingleOptionQuestionViewModel(
            IStatefulInterviewRepository interviewRepository,
            ILiteEventRegistry eventRegistry,
            FilteredOptionsViewModel filteredOptionsViewModel,
            IPrincipal principal,
            QuestionStateViewModel<SingleOptionQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering,
            QuestionInstructionViewModel instructionViewModel) :
            base(principal: principal, questionStateViewModel: questionStateViewModel, answering: answering,
                instructionViewModel: instructionViewModel, interviewRepository: interviewRepository, eventRegistry: eventRegistry)
        {
            this.filteredOptionsViewModel = filteredOptionsViewModel;
        }

        protected override void Initialize(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.filteredOptionsViewModel.Init(interviewId, entityIdentity, SuggestionsMaxCount);
            this.filteredOptionsViewModel.OptionsChanged += FilteredOptionsViewModelOnOptionsChanged;
        }

        private void FilteredOptionsViewModelOnOptionsChanged(object sender, EventArgs eventArgs) => this.SetAnswerAndUpdateFilter();

        protected override IEnumerable<CategoricalOption> GetSuggestions(string filter) 
            => this.filteredOptionsViewModel.GetOptions(filter);

        protected override CategoricalOption GetAnsweredOption(int answer)
            => this.interview.GetOptionForQuestionWithoutFilter(this.Identity, answer);
        protected override CategoricalOption GetOptionByFilter(string filter)
            => this.interview.GetOptionForQuestionWithFilter(this.Identity, filter);

        public override void Dispose()
        {
            base.Dispose();
            this.filteredOptionsViewModel.OptionsChanged -= FilteredOptionsViewModelOnOptionsChanged;
            this.filteredOptionsViewModel.Dispose();
        }
    }
}