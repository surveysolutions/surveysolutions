using System;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class SingleOptionQuestionOptionViewModel : MvxNotifyPropertyChanged
    {
        public event EventHandler BeforeSelected;
        public event EventHandler AnswerRemoved;

        public EnablementViewModel Enablement { get; set; }

        public decimal Value { get; set; }
        public string Title { get; set; }

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
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}