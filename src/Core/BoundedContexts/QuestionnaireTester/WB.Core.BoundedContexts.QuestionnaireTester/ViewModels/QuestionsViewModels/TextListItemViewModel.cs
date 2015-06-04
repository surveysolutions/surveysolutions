using System;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class TextListItemViewModel : MvxNotifyPropertyChanged
    {
        public event EventHandler ItemEdited;

        public event EventHandler ItemDeleted;

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
                try
                {
                    this.title = value;
                    this.OnItemEdited();
                }
                finally
                {
                    this.RaisePropertyChanged();
                }
            }
        }

        public IMvxCommand DeleteListItemCommand
        {
            get { return new MvxCommand(this.DeleteListItem); }
        }

        private void DeleteListItem()
        {
            if (this.ItemDeleted != null) this.ItemDeleted.Invoke(this, EventArgs.Empty);
        }

        private void OnItemEdited()
        {
            if (this.ItemEdited != null) this.ItemEdited.Invoke(this, EventArgs.Empty);
        }
    }
}