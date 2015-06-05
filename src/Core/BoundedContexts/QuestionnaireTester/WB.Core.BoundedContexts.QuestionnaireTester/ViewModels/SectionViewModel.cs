using Cirrious.MvvmCross.ViewModels;

using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class SectionViewModel : MvxNotifyPropertyChanged
    {
        public Identity sectionIdentity;

        private string title;
        private bool isSelected;

        public string Title
        {
            get { return this.title; }
            set { this.title = value; this.RaisePropertyChanged(); }
        }

        public bool IsSelected
        {
            get { return this.isSelected; }
            set { this.isSelected = value; this.RaisePropertyChanged(); }
        }
    }
}