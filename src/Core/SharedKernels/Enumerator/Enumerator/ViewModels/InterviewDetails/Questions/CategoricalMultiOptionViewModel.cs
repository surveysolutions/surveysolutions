using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class CategoricalMultiOptionViewModel : CategoricalMultiOptionViewModel<int>
    {
        private Action setAnswer;
        private bool isRosterSizeQuestion;
        private bool wasChecked;

        private readonly IUserInteractionService userInteraction;

        public CategoricalMultiOptionViewModel(IUserInteractionService userInteraction, AttachmentViewModel attachmentViewModel)
            : base(attachmentViewModel)
        {
            this.userInteraction = userInteraction;
        }

        public override void Init(IQuestionStateViewModel questionState, string sTitle, int value, bool isProtected, Action setAnswer, string attachmentName)
        {
            this.setAnswer = setAnswer;
            base.Init(questionState, sTitle, value, isProtected, async () => await this.SetAnswerAsync(), attachmentName);
        }

        public void MakeRosterSize() => this.isRosterSizeQuestion = true;

        private async Task SetAnswerAsync()
        {
            if (this.wasChecked) return;

            if (this.Checked || !this.isRosterSizeQuestion)
                this.setAnswer.Invoke();
            else if (this.userInteraction.HasPendingUserInteractions)
                this.Checked = true;
            else
            {
                this.wasChecked = true;
                var canRemoveRoster = await this.userInteraction.ConfirmAsync(
                    string.Format(UIResources.Interview_Questions_RemoveRowFromRosterMessage,
                        $"<b>'{this.Title}'</b>"));
                this.wasChecked = false;

                if (!canRemoveRoster)
                    this.Checked = true;
                else
                {
                    this.Checked = false;
                    this.setAnswer.Invoke();
                }
            }
        }

        public override bool IsAnswered() => this.wasChecked || this.Checked;
        public override bool IsOrdered() => this.IsAnswered();
    }
}
