using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class YesNoQuestionOptionViewModel : MvxNotifyPropertyChanged, ICompositeEntity
    {
        private readonly IUserInteractionService userInteraction;

        public YesNoQuestionOptionViewModel(IUserInteractionService userInteraction)
        {
            this.userInteraction = userInteraction;
        }

        public void Init(Identity identity, CategoricalOption model,
            QuestionStateViewModel<YesNoQuestionAnswered> questionState, bool isRosterSizeQuestion, bool isProtected,
            Action setAnswer)
        {
            this.QuestionState = questionState;
            this.IsProtected = isProtected;
            this.Value = model.Value;
            this.Title = model.Title;
            this.IsProtected = isProtected;
            this.YesItemTag = $"{identity}_Opt_Yes_{model.Value}";
            this.NoItemTag = $"{identity}_Opt_No_{model.Value}";

            this.isRosterSizeQuestion = isRosterSizeQuestion;
            this.setAnswer = setAnswer;
        }

        private bool isRosterSizeQuestion;
        private Action setAnswer;

        private string title;
        public string Title
        {
            get => this.title;
            set => this.SetProperty(ref this.title, value);
        }

        private bool yesSelected;
        public bool YesSelected
        {
            get => this.yesSelected;
            set => this.SetProperty(ref this.yesSelected, value);
        }

        private bool noSelected;
        public bool NoSelected
        {
            get => this.noSelected;
            set => this.SetProperty(ref this.noSelected, value);
        }

        private int? yesAnswerCheckedOrder;
        public int? YesAnswerCheckedOrder
        {
            get => this.yesAnswerCheckedOrder;
            set => this.SetProperty(ref this.yesAnswerCheckedOrder, value);
        }

        private bool yesCanBeChecked = true;
        public bool YesCanBeChecked
        {
            get => this.yesCanBeChecked;
            set => this.SetProperty(ref yesCanBeChecked, value);
        }

        public QuestionStateViewModel<YesNoQuestionAnswered> QuestionState { get; private set; }
        public decimal Value { get; private set; }
        public bool IsProtected { get; private set; }
        public string YesItemTag { get; private set; }
        public string NoItemTag { get; private set; }

        public IMvxCommand SetYesAnswerCommand => new MvxCommand(SetYesAsync);

        public IMvxAsyncCommand SetNoAnswerCommand => new MvxAsyncCommand(SetNoAsync);

        public IMvxAsyncCommand RemoveAnswerCommand => new MvxAsyncCommand(RemoveAnswerAsync);

        private async Task RemoveAnswerAsync()
        {
            if (!await this.HasConfirmationByRemovingRosterInstanceAsync()) return;

            this.YesSelected = false;
            this.NoSelected = false;

            this.setAnswer.Invoke();
        }

        private void SetYesAsync()
        {
            this.NoSelected = false;
            this.setAnswer?.Invoke();
        }

        private async Task SetNoAsync()
        {
            var prevState = new {isYes = this.YesSelected, isNo = false};

            if (await this.HasConfirmationByRemovingRosterInstanceAsync())
            {
                this.NoSelected = true;
                this.YesSelected = false;
                this.setAnswer?.Invoke();
            }
            else
            {
                this.NoSelected = prevState.isNo;
                this.YesSelected = prevState.isYes;
            }
        }

        private async Task<bool> HasConfirmationByRemovingRosterInstanceAsync()
        {
            if (!this.isRosterSizeQuestion) return true;
            if (!this.YesSelected) return true;

            if (this.userInteraction.HasPendingUserInteractions) return false;

            return await this.userInteraction.ConfirmAsync(UIResources.Interview_Questions_RemoveRowFromRosterMessage);
        }
    }
}
