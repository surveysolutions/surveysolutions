namespace WB.Core.BoundedContexts.Capi.ViewModel
{
    public class InterviewGroup : InterviewEntity
    {
        public short Level { get; set; }

        private string title;
        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                RaisePropertyChanged(() => Title);
            }
        }
    }
}