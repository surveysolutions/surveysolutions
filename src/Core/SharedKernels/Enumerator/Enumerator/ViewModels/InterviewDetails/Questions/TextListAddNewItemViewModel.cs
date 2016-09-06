﻿using System;
using MvvmCross.Core.ViewModels;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class TextListAddNewItemViewModel : MvxNotifyPropertyChanged, ICompositeEntity
    {
        private readonly QuestionStateViewModel<TextListQuestionAnswered> questionState;
        public IQuestionStateViewModel QuestionState => this.questionState;

        public TextListAddNewItemViewModel(QuestionStateViewModel<TextListQuestionAnswered> questionState)
        {
            this.questionState = questionState;
        }
        
        public event EventHandler ItemEdited;

        private string text;
        public string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                this.text = value;
                this.RaisePropertyChanged();
            }
        }
        
        public IMvxCommand AddNewItemCommand => new MvxCommand(this.AddNewItem);

        private void AddNewItem()
        {
            if (string.IsNullOrWhiteSpace(this.Text)) return;

            this.ItemEdited?.Invoke(this, EventArgs.Empty);
        }
    }
}