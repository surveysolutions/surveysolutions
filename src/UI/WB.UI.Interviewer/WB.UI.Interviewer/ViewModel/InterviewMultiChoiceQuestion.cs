using System.Collections.ObjectModel;

namespace WB.UI.Interviewer.ViewModel
{
    public class InterviewMultiChoiceQuestion : InterviewQuestion
    {
        public bool Ordered { get; set; }
        public int MaxAnswers { get; set; }

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