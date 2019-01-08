using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class MultiOptionQuestionOptionViewModel : MultiOptionQuestionOptionViewModelBase
    {
        private readonly IUserInteractionService userInteraction;

        public MultiOptionQuestionOptionViewModel(IMultiOptionQuestionViewModelToggleable questionViewModel,
            IUserInteractionService userInteractionService) : base(questionViewModel)
        {
            this.userInteraction = userInteractionService;
        }

        public int Value { get; set; }

        public QuestionStateViewModel<MultipleOptionsQuestionAnswered> QuestionState { get; set; }

        public string ItemTag => this.QuestionViewModel.QuestionIdentity + "_Opt_" + Value;

        protected override async Task CheckAnswerAsync()
        {
            if (await this.HasConfirmationByRemovingRosterInstanceAsync())
            {
                base.SortCheckedOptions();
                await this.QuestionViewModel.ToggleAnswerAsync(this);
            }
            else this.Checked = !this.Checked;
        }

        private async Task<bool> HasConfirmationByRemovingRosterInstanceAsync()
        {
            if (!this.IsRosterSize) return true;
            if (this.Checked) return true;

            if (this.userInteraction.HasPendingUserInteractions) return false;

            return await this.userInteraction.ConfirmAsync(UIResources.Interview_Questions_RemoveRowFromRosterMessage);
        }
    }
}
