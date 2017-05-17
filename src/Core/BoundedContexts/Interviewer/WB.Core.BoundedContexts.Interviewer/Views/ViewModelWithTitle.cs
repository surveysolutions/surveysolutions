using MvvmCross.Core.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class ViewModelWithTitle : MvxViewModel
    {
        private string title;
        public string Title
        {
            get { return this.title; }
            set { this.title = value; this.RaisePropertyChanged(); }
        }
    }
}