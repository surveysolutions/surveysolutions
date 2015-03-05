namespace WB.Core.BoundedContexts.Capi.ViewModel
{
    public class InterviewRoster : InterviewGroup { }

    public class InteviewStaticText : InterviewEntity
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