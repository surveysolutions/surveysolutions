using System;
using System.Collections.Generic;
using MvvmCross.Core.ViewModels;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class SingleOptionQuestionOptionViewModel : MvxNotifyPropertyChanged,
        ICompositeEntity
    {
        public event EventHandler BeforeSelected;
        public event EventHandler AnswerRemoved;
        public EnablementViewModel Enablement { get; set; }

        public decimal Value { get; set; }

        private string title;
        public string Title
        {
            get { return this.title; }
            set { this.RaiseAndSetIfChanged(ref this.title, value); }
        }

        private bool selected;

        public bool Selected
        {
            get { return this.selected; }

            set
            {
                if (value)
                {
                    this.OnBeforeSelected();
                }

                this.selected = value;
                this.RaisePropertyChanged();
            }
        }


        public IMvxCommand RemoveAnswerCommand
        {
            get
            {
                return new MvxCommand(OnAnswerRemoved);
            }
        }

        public QuestionStateViewModel<SingleOptionQuestionAnswered> QuestionState { get; set; }

        private void OnBeforeSelected()
        {
            if (this.BeforeSelected != null) this.BeforeSelected.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnAnswerRemoved()
        {
            var handler = this.AnswerRemoved;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }

    public class SingleOptionQuestionOptionViewModelEqualityComparer : IEqualityComparer<SingleOptionQuestionOptionViewModel>
    {
        public bool Equals(SingleOptionQuestionOptionViewModel x, SingleOptionQuestionOptionViewModel y)
        {
            return (x.Title == y.Title
                    && x.Value == y.Value
                    && x.Selected == y.Selected);
        }

        public int GetHashCode(SingleOptionQuestionOptionViewModel obj)
        {
            return obj?.Value.GetHashCode() ?? 0;
        }
    }
}