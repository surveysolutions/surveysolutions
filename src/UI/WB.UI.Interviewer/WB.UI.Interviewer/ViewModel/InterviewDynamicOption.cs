using Cirrious.MvvmCross.ViewModels;

namespace WB.Core.BoundedContexts.Capi.ViewModel
{
    public class InterviewDynamicOption<T> : MvxNotifyPropertyChanged
    {
        public T Value { get; set; }

        private string label;
        public string Label
        {
            get { return label; }
            set
            {
                label = value;
                RaisePropertyChanged(() => Label);
            }
        }

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