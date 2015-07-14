using System;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Tester.ViewModels.Questions.State;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.BoundedContexts.Tester.ViewModels.Questions
{
    public class SingleOptionQuestionOptionViewModel : MvxNotifyPropertyChanged
    {
        public event EventHandler BeforeSelected;

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

        public QuestionStateViewModel<SingleOptionQuestionAnswered> QuestionState { get; set; }

        private void OnBeforeSelected()
        {
            if (this.BeforeSelected != null) this.BeforeSelected.Invoke(this, EventArgs.Empty);
        }
    }
}