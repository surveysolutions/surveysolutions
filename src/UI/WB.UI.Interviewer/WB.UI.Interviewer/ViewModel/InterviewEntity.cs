using Cirrious.MvvmCross.ViewModels;

namespace WB.Core.BoundedContexts.Capi.ViewModel
{
    public class InterviewEntity : MvxNotifyPropertyChanged
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