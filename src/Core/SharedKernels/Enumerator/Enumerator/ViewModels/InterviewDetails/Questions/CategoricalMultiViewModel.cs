using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Base;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class CategoricalMultiViewModel : CategoricalMultiViewModelBase<int, int>,
        IAsyncViewModelEventHandler<MultipleOptionsQuestionAnswered>
    {
        protected readonly IUserInteractionService userInteraction;
        protected readonly FilteredOptionsViewModel filteredOptionsViewModel;
        protected readonly IInterviewViewModelFactory interviewViewModelFactory;

        public CategoricalMultiViewModel(
            QuestionStateViewModel<MultipleOptionsQuestionAnswered> questionStateViewModel,
            IQuestionnaireStorage questionnaireRepository,
            IViewModelEventRegistry eventRegistry,
            IStatefulInterviewRepository interviewRepository,
            IPrincipal principal,
            IUserInteractionService userInteraction,
            AnsweringViewModel answering,
            FilteredOptionsViewModel filteredOptionsViewModel,
            QuestionInstructionViewModel instructionViewModel,
            ThrottlingViewModel throttlingModel,
            IInterviewViewModelFactory interviewViewModelFactory,
            IMvxMainThreadAsyncDispatcher mainThreadAsyncDispatcher) 
            : base(questionStateViewModel, questionnaireRepository, eventRegistry,
            interviewRepository, principal, answering, instructionViewModel,
            throttlingModel, mainThreadAsyncDispatcher)
        {
            this.userInteraction = userInteraction;
            this.filteredOptionsViewModel = filteredOptionsViewModel;
            this.interviewViewModelFactory = interviewViewModelFactory;
            this.Options = new CovariantObservableCollection<CategoricalMultiOptionViewModel<int>>();
        }

        protected override IEnumerable<CategoricalMultiOptionViewModel<int>> GetOptions(IStatefulInterview interview)
        {
            foreach (var categoricalOption in this.filteredOptionsViewModel.GetOptions())
            {
                var vm = interviewViewModelFactory.GetNew<CategoricalMultiOptionViewModel>();
                base.InitViewModel(categoricalOption.Title, categoricalOption.Value, interview, vm, 
                    categoricalOption.AttachmentName, interview.IsAnswerProtected(this.Identity, categoricalOption.Value));

                if(this.isRosterSizeQuestion) vm.MakeRosterSize();

                yield return vm;
            }
        }

        protected override bool IsInterviewAnswer(int interviewAnswer, int optionValue)
            => interviewAnswer == optionValue;

        protected override void Init(IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            this.isRosterSizeQuestion = questionnaire.IsRosterSizeQuestion(this.Identity.Id);

            this.filteredOptionsViewModel.Init(interview.Id.FormatGuid(), this.Identity);
            filteredOptionsViewModel.OptionsChanged += FilteredOptionsViewModelOnOptionsChanged;
        }

        protected int[] selectedOptionsToSave;
        protected bool isRosterSizeQuestion;
        
        protected override void SaveAnsweredOptionsForThrottling(IOrderedEnumerable<CategoricalMultiOptionViewModel<int>> answeredViewModels) 
            => this.selectedOptionsToSave = answeredViewModels.Select(x => x.Value).ToArray();
        
        protected override void SetAnswerToOptionViewModel(CategoricalMultiOptionViewModel<int> optionViewModel, int answer)
            => optionViewModel.Checked = answer == optionViewModel.Value;

        protected override void RemoveAnswerFromOptionViewModel(CategoricalMultiOptionViewModel<int> optionViewModel)
            => optionViewModel.Checked = false;

        protected override AnswerQuestionCommand GetAnswerCommand(Guid interviewId, Guid userId) =>
            new AnswerMultipleOptionsQuestionCommand(interviewId, userId, this.Identity.Id, this.Identity.RosterVector, this.selectedOptionsToSave);
        
        protected override int[] GetAnsweredOptionsFromInterview(IStatefulInterview interview) 
            => interview.GetMultiOptionQuestion(this.Identity).GetAnswer()?.CheckedValues?.ToArray();
        
        public virtual async Task HandleAsync(MultipleOptionsQuestionAnswered @event)
        {
            if (@event.QuestionId != this.Identity.Id 
                || !@event.RosterVector.Identical(this.Identity.RosterVector)
                || throttlingModel.HasPendingAction) 
                return;

            await this.UpdateViewModelsByAnsweredOptionsAsync(@event.SelectedValues?.Select(Convert.ToInt32)?.ToArray());
        }

        private async Task FilteredOptionsViewModelOnOptionsChanged(object sender, EventArgs e)
            => await this.UpdateViewModelsAsync();

        public override void Dispose()
        {
            this.filteredOptionsViewModel.OptionsChanged -= this.FilteredOptionsViewModelOnOptionsChanged;
            this.filteredOptionsViewModel.Dispose();

            base.Dispose();
        }
    }
}
