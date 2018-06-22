using System;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class YesNoQuestionOptionViewModel : MvxNotifyPropertyChanged, ICompositeEntity
    {
        public YesNoQuestionViewModel QuestionViewModel { get; private set; }
        public QuestionStateViewModel<YesNoQuestionAnswered> QuestionState { get; set; }

        private event EventHandler<EventArgs> AnswerChanged; 

        public YesNoQuestionOptionViewModel(YesNoQuestionViewModel questionViewModel,
            QuestionStateViewModel<YesNoQuestionAnswered> questionState)
        {
            this.QuestionViewModel = questionViewModel;
            this.QuestionState = questionState;
            this.AnswerChanged += (o, e) => this.RaiseToggleAnswer(e as YesNoEventArgs);
        }

        public decimal Value { get; set; }

        private string title;
        public string Title
        {
            get => this.title;
            set { this.title = value; this.RaisePropertyChanged(); }
        }

        private bool? selected;

        public bool? Selected
        {
            get => this.selected;
            set
            {
                if (this.selected == value)
                    return;

                this.selected = value;

                this.RaisePropertyChanged();
                this.RaisePropertyChanged(() => YesSelected);
                this.RaisePropertyChanged(() => NoSelected);
            }
        }

        public bool YesSelected
        {
            get => this.Selected.HasValue && this.Selected.Value;
            set
            {
                if (this.YesSelected == value)
                    return;

                var oldValue = this.Selected;
                this.Selected = value;
                this.OnAnswerChanged(oldValue);
            }
        }

        public bool NoSelected
        {
            get => this.Selected.HasValue && !this.Selected.Value;
            set
            {
                if (this.NoSelected == value)
                    return;

                var oldValue = this.Selected;
                this.Selected = !value;
                this.OnAnswerChanged(oldValue);
            }
        }

        private int? yesAnswerCheckedOrder;
        private bool yesCanBeChecked = true;

        public int? YesAnswerCheckedOrder
        {
            get => this.yesAnswerCheckedOrder;
            set
            {
                if (this.yesAnswerCheckedOrder == value)
                    return;

                this.yesAnswerCheckedOrder = value;
                this.RaisePropertyChanged();
            }
        }

        public int? AnswerCheckedOrder { get; set; }

        public bool IsProtected { get; set; }

        public bool YesCanBeChecked
        {
            get => yesCanBeChecked;
            set => SetProperty(ref yesCanBeChecked, value);
        }

        public async void RaiseToggleAnswer(YesNoEventArgs e)
        {
            await this.QuestionViewModel.ToggleAnswerAsync(this, e.OldValue).ConfigureAwait(false); 
        }

        public IMvxCommand RemoveAnswerCommand
        {
            get
            {
                return new MvxCommand(() =>
                {
                    bool? oldValue = this.Selected;
                    this.Selected = null;
                    this.OnAnswerChanged(oldValue);
                });
            }
        }

        protected virtual void OnAnswerChanged(bool? oldValue)
        {
            this.AnswerChanged?.Invoke(this, new YesNoEventArgs(oldValue));
        }

        public string YesItemTag => this.QuestionViewModel.Identity + "_Opt_Yes_" + Value;
        public string NoItemTag => this.QuestionViewModel.Identity + "_Opt_No_" + Value;
    }

    public class YesNoEventArgs : EventArgs
    {
        public bool? OldValue { get; }

        public YesNoEventArgs(bool? oldValue)
        {
            OldValue = oldValue;
        }
    } 
}
