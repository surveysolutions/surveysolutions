using System.Collections.ObjectModel;

namespace WB.Core.BoundedContexts.Capi.ViewModel
{
    public class InterviewSingleChoiceQuestion : InterviewQuestion
    {
        private ObservableCollection<InterviewStaticOption> options;
        public ObservableCollection<InterviewStaticOption> Options
        {
            get { return options; }
            set
            {
                options = value;
                RaisePropertyChanged(() => Options);
            }
        }
    }
}