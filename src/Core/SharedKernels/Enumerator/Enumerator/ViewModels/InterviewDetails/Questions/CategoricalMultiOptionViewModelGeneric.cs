using System;
using MvvmCross.Commands;
using MvvmCross.ViewModels;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class CategoricalMultiOptionViewModel<T> : MvxNotifyPropertyChanged, ICompositeEntity
    {
        public virtual void Init(IQuestionStateViewModel questionState, string sTitle, T value, bool isProtected, Action setAnswer)
        {
            this.QuestionState = questionState;
            this.IsProtected = isProtected;
            this.Value = value;
            this.Title = sTitle;
            this.IsProtected = isProtected;
            this.ItemTag = $"{questionState.Header.Identity}_Opt_{(value == null ? "<null>" : value.ToString())}";

            this.setAnswer = setAnswer;
        }

        private Action setAnswer;

        public T Value { get; private set; }
        public string ItemTag { get; private set; }
        public bool IsProtected { get; private set; }
        public IQuestionStateViewModel QuestionState { get; private set; }

        private string title;
        public string Title
        {
            get => this.title;
            set => this.SetProperty(ref this.title, value);
        }

        private bool @checked;
        public bool Checked
        {
            get => this.@checked;
            set => this.SetProperty(ref this.@checked, value);
        }

        private int? checkedOrder;
        public int? CheckedOrder
        {
            get => this.checkedOrder;
            set => this.SetProperty(ref this.checkedOrder, value);
        }

        private bool canBeChecked = true;
        public bool CanBeChecked
        {
            get => this.canBeChecked;
            set => SetProperty(ref this.canBeChecked, value);
        }
        
        public IMvxCommand CheckAnswerCommand => new MvxCommand(this.setAnswer, () => CanBeChecked && !IsProtected);
    }
}
