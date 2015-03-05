using System;

namespace WB.Core.BoundedContexts.Capi.ViewModel
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