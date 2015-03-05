using Cirrious.MvvmCross.ViewModels;

namespace WB.Core.BoundedContexts.Capi.ViewModel
{
    public class InterviewStaticOption : MvxNotifyPropertyChanged
    {
        public string Label { get; set; }
        public string Value { get; set; }

        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                RaisePropertyChanged(() => IsSelected);
            }
        }
    }
}