namespace WB.Core.BoundedContexts.Capi.ViewModel
{
    public class InterviewQrBarcodeQuestion : InterviewQuestion
    {
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