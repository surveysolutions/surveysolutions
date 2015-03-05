using System.Collections.ObjectModel;

namespace WB.Core.BoundedContexts.Capi.ViewModel
{
    public class InterviewLinkedSinleChoiceQuestion<T> : InterviewQuestion
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