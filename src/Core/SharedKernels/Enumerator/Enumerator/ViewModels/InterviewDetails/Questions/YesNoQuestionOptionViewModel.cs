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
            this.AnswerChanged += (o, e) => this.RaiseToggleAnswer();
        }

        public decimal Value { get; set; }

        private string title;
        public string Title
        {
            get { return this.title; }
            set { this.title = value; this.RaisePropertyChanged(); }
        }

        private bool? selected;

        public bool? Selected
        {
            get { return this.selected; }
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
            get { return this.Selected.HasValue && this.Selected.Value; }
            set
            {
                if (this.YesSelected == value)
                    return;

                this.Selected = value;
                this.OnAnswerChanged();
            }
        }

        public bool NoSelected
        {
            get { return this.Selected.HasValue && !this.Selected.Value; }
            set
            {
                if (this.NoSelected == value)
                    return;

                this.Selected = !value;
                this.OnAnswerChanged();
            }
        }

        private int? yesAnswerCheckedOrder;
        private bool yesCanBeChecked = true;

        public int? YesAnswerCheckedOrder
        {
            get { return this.yesAnswerCheckedOrder; }
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

        public async void RaiseToggleAnswer()
        {
            await this.QuestionViewModel.ToggleAnswerAsync(this).ConfigureAwait(false); 
        }

        public IMvxCommand RemoveAnswerCommand
        {
            get
            {
                return new MvxCommand(() => {
                    this.Selected = null;
                    this.OnAnswerChanged();
                });
            }
        }

        protected virtual void OnAnswerChanged()
        {
            this.AnswerChanged?.Invoke(this, EventArgs.Empty);
        }

        public string YesItemTag => this.QuestionViewModel.Identity + "_Opt_Yes_" + Value;
        public string NoItemTag => this.QuestionViewModel.Identity + "_Opt_No_" + Value;

    }
}
