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
        private string itemTag;

        public bool Selected
        {
            get { return this.selected; }

            set
            {
                try
                {
                    if (value)
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

        public IMvxCommand RemoveAnswerCommand => new MvxCommand(this.OnAnswerRemoved);

        public QuestionStateViewModel<SingleOptionLinkedQuestionAnswered> QuestionState { get; set; }

        public string ItemTag => itemTag ?? (itemTag = this.QuestionState.Header.Identity + "_Opt_" + string.Join(",", this.RosterVector));

        private void OnBeforeSelected()
        {
            this.BeforeSelected?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnAnswerRemoved()
        {
            var handler = this.AnswerRemoved;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}