namespace WB.Core.BoundedContexts.Capi.ViewModel
{
    public class InterviewGeolocationQuestion : InterviewQuestion
    {
        private InterviewGeoLocation answer;
        public InterviewGeoLocation Answer
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