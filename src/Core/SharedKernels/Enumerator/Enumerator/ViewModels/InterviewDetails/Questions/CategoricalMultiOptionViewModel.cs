using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class CategoricalMultiOptionViewModel : CategoricalMultiOptionViewModel<int>
    {
        public void MakeRosterSize() => this.isRosterSizeQuestion = true;

        private Action setAnswer;
        private bool isRosterSizeQuestion;

        private readonly IUserInteractionService userInteraction;

        public CategoricalMultiOptionViewModel(IUserInteractionService userInteraction)
        {
            this.userInteraction = userInteraction;
        }

        public override void Init(IQuestionStateViewModel questionState, string sTitle, int value, bool isProtected, Action setAnswer)
        {
            this.setAnswer = setAnswer;
            base.Init(questionState, sTitle, value, isProtected, async () => await this.SetAnswerAsync());
        }

        private async Task SetAnswerAsync()
        {
            if (this.Checked || !this.isRosterSizeQuestion)
                this.setAnswer.Invoke();
            else if (this.userInteraction.HasPendingUserInteractions)
                this.Checked = true;
            else if(!await this.userInteraction.ConfirmAsync(UIResources.Interview_Questions_RemoveRowFromRosterMessage))
                this.Checked = true;
            else
            {
                this.Checked = false;
                this.setAnswer.Invoke();
            }
        }
    }
}
