namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class DashboardTabMenuViewModel
    {
        public DashboardTabMenuViewModel(int count)
        {
            this.Count = count;
        }

        public int Count { get; set; }

        public bool IsVisible
        {
            get { return this.Count > 0; }
        }

    }
}