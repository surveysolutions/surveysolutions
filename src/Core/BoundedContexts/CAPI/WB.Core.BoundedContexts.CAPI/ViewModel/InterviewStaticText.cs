namespace WB.Core.BoundedContexts.Capi.ViewModel
{
    public class InterviewStaticText : InterviewEntity
    {
        private string text;
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                RaisePropertyChanged(() => Text);
            }
        }
    }
}