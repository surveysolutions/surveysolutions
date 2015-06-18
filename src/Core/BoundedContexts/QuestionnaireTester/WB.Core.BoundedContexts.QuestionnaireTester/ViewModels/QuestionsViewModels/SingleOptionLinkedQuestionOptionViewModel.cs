using System;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class SingleOptionLinkedQuestionOptionViewModel : MvxNotifyPropertyChanged
    {
        public event EventHandler BeforeSelected;

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

        private void OnBeforeSelected()
        {
            if (this.BeforeSelected != null) this.BeforeSelected.Invoke(this, EventArgs.Empty);
        }
    }
}