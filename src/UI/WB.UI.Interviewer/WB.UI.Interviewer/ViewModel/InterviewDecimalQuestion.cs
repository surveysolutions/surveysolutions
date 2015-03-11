namespace WB.UI.Interviewer.ViewModel
{
    public class InterviewDecimalQuestion : InterviewQuestion
    {
        public int Precision { get; set; }

        private decimal answer;
        public decimal Answer
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