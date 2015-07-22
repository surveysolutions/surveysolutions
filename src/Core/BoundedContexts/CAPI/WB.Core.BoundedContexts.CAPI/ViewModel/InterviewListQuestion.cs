using System.Collections.ObjectModel;

namespace WB.Core.BoundedContexts.Capi.ViewModel
{
    public class InterviewListQuestion : InterviewQuestion
    {
        public int MaxAnswers { get; set; }

        private ObservableCollection<string> answers;
        public ObservableCollection<string> Answers
        {
            get { return answers; }
            set
            {
                answers = value;
                RaisePropertyChanged(() => Answers);
            }
        }
    }
}