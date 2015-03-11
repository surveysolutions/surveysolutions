using System;

namespace WB.UI.Interviewer.ViewModel
{
    public class InterviewDateQuestion : InterviewQuestion
    {
        private DateTime answer;
        public DateTime Answer
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