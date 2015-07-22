using System.Collections.ObjectModel;

namespace WB.Core.BoundedContexts.Capi.ViewModel
{
    public class InterviewLinkedMultiChoiceQuestion<T> : InterviewQuestion
    {
        public bool Ordered { get; set; }
        public int MaxAnswers { get; set; }

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