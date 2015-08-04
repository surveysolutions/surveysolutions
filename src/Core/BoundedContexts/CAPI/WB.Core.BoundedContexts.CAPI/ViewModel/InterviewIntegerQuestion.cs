namespace WB.Core.BoundedContexts.Capi.ViewModel
{
    public class InterviewIntegerQuestion : InterviewQuestion
    {
        public int MaxValue { get; set; }

        private int answer;
        public int Answer
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