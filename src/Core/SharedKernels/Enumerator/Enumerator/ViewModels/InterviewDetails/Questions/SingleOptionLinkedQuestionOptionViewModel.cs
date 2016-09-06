using System;
using MvvmCross.Core.ViewModels;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class SingleOptionLinkedQuestionOptionViewModel : MvxNotifyPropertyChanged,
        ICompositeEntity
    {
        public event EventHandler BeforeSelected;
        public event EventHandler AnswerRemoved;
        public EnablementViewModel Enablement { get; set; }

        public decimal[] RosterVector { get; set; }

        private string title;
        public string Title
        {
            get { return this.title; }
            set { this.title = value; this.RaisePropertyChanged(); }
        }

        private bool selected;

        public bool Selected
        {
            get { return this.selected; }

            set
            {
                try
                {
                    if (value == true)
                    {
                        this.OnBeforeSelected();
                    }

                    this.selected = value;
                }
                finally
                {
                    this.RaisePropertyChanged();
                }
            }
        }

        public IMvxCommand RemoveAnswerCommand
        {
            get
            {
                return new MvxCommand(OnAnswerRemoved);
            }
        }

        public QuestionStateViewModel<SingleOptionLinkedQuestionAnswered> QuestionState { get; set; }

        private void OnBeforeSelected()
        {
            if (this.BeforeSelected != null) this.BeforeSelected.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnAnswerRemoved()
        {
            var handler = this.AnswerRemoved;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}