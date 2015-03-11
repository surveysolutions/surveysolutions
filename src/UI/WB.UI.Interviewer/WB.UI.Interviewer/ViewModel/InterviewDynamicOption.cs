using GalaSoft.MvvmLight;

namespace WB.UI.Interviewer.ViewModel
{
    public class InterviewDynamicOption<T> : ObservableObject
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