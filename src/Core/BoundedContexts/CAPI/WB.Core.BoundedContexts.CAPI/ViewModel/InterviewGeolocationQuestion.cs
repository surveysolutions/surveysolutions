namespace WB.Core.BoundedContexts.Capi.ViewModel
{
    public class InterviewGeolocationQuestion : InterviewQuestion
    {
        private GeoLocation answer;
        public GeoLocation Answer
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