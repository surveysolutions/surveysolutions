using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class CategoricalYesNoViewModel : CategoricalMultiViewModelBase<decimal, AnsweredYesNoOption>,
        IAsyncViewModelEventHandler<YesNoQuestionAnswered>
    {
        private readonly FilteredOptionsViewModel filteredOptionsViewModel;
        private readonly IInterviewViewModelFactory interviewViewModelFactory;

        private bool isRosterSizeQuestion;
        private AnsweredYesNoOption[] selectedOptionsToSave;
        
        public CategoricalYesNoViewModel(QuestionStateViewModel<YesNoQuestionAnswered> questionStateViewModel,
            IQuestionnaireStorage questionnaireRepository, IViewModelEventRegistry eventRegistry,
            IStatefulInterviewRepository interviewRepository, IPrincipal principal, IUserInteractionService userInteraction,
            AnsweringViewModel answering, QuestionInstructionViewModel instructionViewModel, ThrottlingViewModel throttlingModel,
            FilteredOptionsViewModel filteredOptionsViewModel, IInterviewViewModelFactory interviewViewModelFactory)
            : base(questionStateViewModel, questionnaireRepository, eventRegistry, interviewRepository, principal,
                answering, instructionViewModel, throttlingModel)
        {
            this.filteredOptionsViewModel = filteredOptionsViewModel;
            this.interviewViewModelFactory = interviewViewModelFactory;
            this.Options = new CovariantObservableCollection<CategoricalMultiOptionViewModel<decimal>>();
        }

        protected override bool IsInterviewAnswer(AnsweredYesNoOption interviewAnswer, decimal optionValue)
            => interviewAnswer.OptionValue == optionValue;

        protected override void Init(IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            this.isRosterSizeQuestion = questionnaire.IsRosterSizeQuestion(this.Identity.Id);

            this.filteredOptionsViewModel.Init(interview.Id.FormatGuid(), this.Identity);
            filteredOptionsViewModel.OptionsChanged += FilteredOptionsViewModelOnOptionsChanged;
        }
        private async Task FilteredOptionsViewModelOnOptionsChanged(object sender, EventArgs e)
            => await this.UpdateViewModelsAsync();

        protected override void SaveAnsweredOptionsForThrottling(IOrderedEnumerable<CategoricalMultiOptionViewModel<decimal>> answeredViewModels) 
            => this.selectedOptionsToSave = answeredViewModels.Select(x => new AnsweredYesNoOption(x.Value, x.Checked)).ToArray();

        protected override AnsweredYesNoOption[] FilterAnsweredOptions(AnsweredYesNoOption[] answeredOptions)
            => answeredOptions.Where(x => x.Yes).ToArray();

        protected override AnsweredYesNoOption[] GetAnsweredOptionsFromInterview(IStatefulInterview interview)
        => interview.GetYesNoQuestion(this.Identity).GetAnswer()?.ToAnsweredYesNoOptions().ToArray();

        protected override void SetAnswerToOptionViewModel(CategoricalMultiOptionViewModel<decimal> optionViewModel, AnsweredYesNoOption answer)
        {
            var yesNoViewModel = (CategoricalYesNoOptionViewModel) optionViewModel;

            yesNoViewModel.Checked = answer?.Yes == true;
            yesNoViewModel.NoSelected = answer?.Yes == false;
        }

        protected override void RemoveAnswerFromOptionViewModel(CategoricalMultiOptionViewModel<decimal> optionViewModel)
        {
            var yesNoViewModel = (CategoricalYesNoOptionViewModel) optionViewModel;
            yesNoViewModel.Checked = false;
            yesNoViewModel.NoSelected = false;
        }

        protected override AnswerQuestionCommand GetAnswerCommand(Guid interviewId, Guid userId)
        => new AnswerYesNoQuestion(interviewId, userId, this.Identity.Id, this.Identity.RosterVector, this.selectedOptionsToSave);

        protected override IEnumerable<CategoricalMultiOptionViewModel<decimal>> GetOptions(IStatefulInterview interview)
        {
            foreach (var categoricalOption in this.filteredOptionsViewModel.GetOptions())
            {
                var vm = interviewViewModelFactory.GetNew<CategoricalYesNoOptionViewModel>();
                base.InitViewModel(categoricalOption.Title, categoricalOption.Value, interview, vm,
                    categoricalOption.AttachmentName,
                    interview.IsAnswerProtected(this.Identity, categoricalOption.Value));

                if (isRosterSizeQuestion) vm.MakeRosterSize();

                yield return vm;
            }
        }

        public async Task HandleAsync(YesNoQuestionAnswered @event)
        {
            if (@event.QuestionId != this.Identity.Id 
                || !@event.RosterVector.Identical(this.Identity.RosterVector)
                || throttlingModel.HasPendingAction) 
                return;
            
            await this.UpdateViewModelsByAnsweredOptionsAsync(@event.AnsweredOptions);
        }

        public override void Dispose()
        {
            this.filteredOptionsViewModel.OptionsChanged -= this.FilteredOptionsViewModelOnOptionsChanged;
            this.filteredOptionsViewModel.Dispose();

            base.Dispose();
        }
    }
}
