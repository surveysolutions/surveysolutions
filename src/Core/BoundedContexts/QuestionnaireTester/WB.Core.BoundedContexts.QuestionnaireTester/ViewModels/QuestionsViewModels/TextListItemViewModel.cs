using System;

using Cirrious.MvvmCross.ViewModels;

using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class TextListItemViewModel : MvxNotifyPropertyChanged
    {
        public event EventHandler BeforeItemEdited;

        public EnablementViewModel Enablement { get; set; }

        public decimal Value { get; set; }

        private string title;
        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                this.title = value;
                this.RaisePropertyChanged();
            }
        }

        private void OnBeforeItemEdited()
        {
            if (this.BeforeItemEdited != null) this.BeforeItemEdited.Invoke(this, EventArgs.Empty);
        }
    }
}