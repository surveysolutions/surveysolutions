using System.Collections.ObjectModel;
using WB.Core.BoundedContexts.Capi.ViewModel;

namespace WB.UI.Interviewer.ViewModel
{
    public class InterviewLinkedSingleChoiceQuestion<T> : InterviewQuestion
    {
        private ObservableCollection<InterviewDynamicOption<T>> options;
        public ObservableCollection<InterviewDynamicOption<T>> Options
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