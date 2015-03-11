using WB.UI.Interviewer.ViewModel;

namespace WB.Core.BoundedContexts.Capi.ViewModel
{
    public class InterviewTextQuestion : InterviewQuestion
    {
        public string Mask { get; set; }

        private string answer;
        public string Answer
        {
            get { return answer; }
            set
            {
                answer = value;
                RaisePropertyChanged(() => Answer);
            }
        }
    }
}