using GalaSoft.MvvmLight;

namespace WB.UI.Interviewer.ViewModel
{
    public class InterviewEntity : ObservableObject
    {
        public string Id { get; set; }
        public string ParentId { get; set; }
        public decimal[] RosterVector { get; set; }

        private bool enabled;
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                RaisePropertyChanged(() => Enabled);
            }
        }
    }
}