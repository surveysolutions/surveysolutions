using System;
using System.Threading.Tasks;
using MvvmCross.Commands;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class CategoricalYesNoOptionViewModel : CategoricalMultiOptionViewModel<decimal>
    {
        private readonly IUserInteractionService userInteraction;

        public CategoricalYesNoOptionViewModel(IUserInteractionService userInteraction, AttachmentViewModel attachmentViewModel)
            : base(attachmentViewModel)
        {
            this.userInteraction = userInteraction;
        }

        public override void Init(IQuestionStateViewModel questionState, string sTitle, decimal value, bool isProtected, Action setAnswer, string attachmentName)
        {
            this.setAnswer = setAnswer;
            base.Init(questionState, sTitle, value, isProtected, SetYes, attachmentName);
        }

        public void MakeRosterSize() => this.isRosterSizeQuestion = true;

        private Action setAnswer;
        private bool isRosterSizeQuestion;
        
        private bool noWasChanged = false;
        private bool noSelected;
        public bool NoSelected
        {
            get => this.noSelected;
            set => noWasChanged = this.SetProperty(ref this.noSelected, value);
        }
        
        public IMvxAsyncCommand SetNoAnswerCommand => new MvxAsyncCommand(SetNoAsync);

        public IMvxAsyncCommand RemoveAnswerCommand => new MvxAsyncCommand(RemoveAnswerAsync);

        private async Task RemoveAnswerAsync()
        {
            if (!await this.HasConfirmationByRemovingRosterInstanceAsync()) return;

            this.Checked = false;
            this.NoSelected = false;

            this.setAnswer?.Invoke();
        }

        private async Task SetNoAsync()
        {
            if (!noWasChanged)
                return;

            var prevState = new {isYes = this.Checked, isNo = false};

            if (await this.HasConfirmationByRemovingRosterInstanceAsync())
            {
                this.NoSelected = true;
                this.Checked = false;
                this.setAnswer?.Invoke();
            }
            else
            {
                this.NoSelected = prevState.isNo;
                this.Checked = prevState.isYes;
            }
        }

        private void SetYes()
        {
            this.NoSelected = false;
            this.setAnswer?.Invoke();
        }

        private async Task<bool> HasConfirmationByRemovingRosterInstanceAsync()
        {
            if (!this.isRosterSizeQuestion) return true;
            if (!this.Checked) return true;

            if (this.userInteraction.HasPendingUserInteractions) return false;

            return await this.userInteraction.ConfirmAsync(string.Format(UIResources.Interview_Questions_RemoveRowFromRosterMessage, $"<b>'{this.Title}'</b>"));
        }

        public override bool IsAnswered() => this.Checked || this.NoSelected;
        public override bool IsOrdered() => this.Checked;
    }
}
