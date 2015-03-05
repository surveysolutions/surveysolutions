namespace WB.Core.BoundedContexts.Capi.ViewModel
{
    public class InterviewImageQuestion : InterviewQuestion
    {
        private byte[] answer;
        public byte[] Answer
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