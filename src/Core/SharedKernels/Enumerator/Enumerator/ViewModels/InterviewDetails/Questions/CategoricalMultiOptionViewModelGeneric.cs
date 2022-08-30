using System;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class CategoricalMultiOptionViewModel<T> : MvxNotifyPropertyChanged, ICompositeEntity
    {
        public virtual void Init(IQuestionStateViewModel questionState, string sTitle, T value, bool isProtected, Action setAnswer, string attachmentName)
        {
            this.QuestionState = questionState;
            this.IsProtected = isProtected;
            this.Value = value;
            this.Title = sTitle;
            this.IsProtected = isProtected;
            this.ItemTag = $"{questionState.Header.Identity}_Opt_{value}";

            this.setAnswer = setAnswer;
            this.Attachment.InitAsStatic(questionState.Header.InterviewId, attachmentName);
        }

        public AttachmentViewModel Attachment { get; }


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

        private bool checkedWasChanged = false;
        private bool @checked;
        public bool Checked
        {
            get => this.@checked;
            set
            {
                checkedWasChanged = this.SetProperty(ref this.@checked, value);
            }
        }

        private int? checkedOrder;
        public int? CheckedOrder
        {
            get => this.checkedOrder;
            set => this.SetProperty(ref this.checkedOrder, value);
        }

        private bool canBeChecked = true;

        public CategoricalMultiOptionViewModel(AttachmentViewModel attachment)
        {
            Attachment = attachment;
        }

        public bool CanBeChecked
        {
            get => this.canBeChecked;
            set => SetProperty(ref this.canBeChecked, value);
        }
        
        public IMvxCommand CheckAnswerCommand => new MvxCommand(() =>
        {
            if (checkedWasChanged)
                this.setAnswer();

        }, () => CanBeChecked && !IsProtected);

        public virtual bool IsAnswered() => this.Checked;
        public virtual bool IsOrdered() => this.Checked;
    }
}
