using System;
using System.Linq;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
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

                if (this.QuestionViewModel.AreAnswersOrdered)
                {
                    if (value == true)
                    {
                        this.AnswerCheckedOrder = this.QuestionViewModel.Options
                                                      .Select(x => x.AnswerCheckedOrder ?? 0)
                                                      .DefaultIfEmpty(0)
                                                      .Max() + 1;
                        this.YesAnswerCheckedOrder = this.QuestionViewModel.Options
                                                         .Select(x => x.YesAnswerCheckedOrder ?? 0)
                                                         .DefaultIfEmpty(0)
                                                         .Max() + 1;
                    }
                    else
                    {
                        if (this.AnswerCheckedOrder.HasValue)
                        {
                            this.QuestionViewModel.Options
                                .Where(x => x.AnswerCheckedOrder > this.AnswerCheckedOrder)
                                .ForEach(x => x.AnswerCheckedOrder -= 1);
                        }

                        if (this.YesAnswerCheckedOrder.HasValue)
                        {
                            this.QuestionViewModel.Options
                                .Where(x => x.YesAnswerCheckedOrder > this.YesAnswerCheckedOrder)
                                .ForEach(x => x.YesAnswerCheckedOrder -= 1);
                        }

                        this.AnswerCheckedOrder = null;
                        this.YesAnswerCheckedOrder = null;
                    }
                }

                this.RaisePropertyChanged();
                this.RaisePropertyChanged(() => YesSelected);
                this.RaisePropertyChanged(() => NoSelected);
            }
        }

        public bool YesSelected
        {
            get => this.Selected == true;
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
            get => this.Selected == false;
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
            await this.QuestionViewModel.ToggleAnswerAsync(this, e.OldValue); 
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
