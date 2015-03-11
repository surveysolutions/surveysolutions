using GalaSoft.MvvmLight;

namespace WB.UI.Interviewer.ViewModel
{
    public class InterviewStaticOption : ObservableObject
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