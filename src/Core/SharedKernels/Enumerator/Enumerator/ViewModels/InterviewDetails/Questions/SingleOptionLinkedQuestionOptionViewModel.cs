using System;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;
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
            get => this.title;
            set { this.title = value; this.RaisePropertyChanged(); }
        }

        private bool selected;

        public bool Selected
        {
            get => this.selected;

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

        private void OnBeforeSelected()
        {
            this.BeforeSelected?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnAnswerRemoved()
        {
            var handler = this.AnswerRemoved;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public string ItemTag => this.QuestionState.Header.Identity + "_Opt_" + (RosterVector == null ? "<null>" : new RosterVector(RosterVector).ToString());
    }
}
