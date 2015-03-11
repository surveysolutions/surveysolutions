using System.Collections.ObjectModel;
using WB.Core.BoundedContexts.Capi.ViewModel;

namespace WB.UI.Interviewer.ViewModel
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