using WB.Core.BoundedContexts.Capi.ViewModel;

namespace WB.UI.Interviewer.ViewModel
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