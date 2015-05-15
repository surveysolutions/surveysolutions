using System;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
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