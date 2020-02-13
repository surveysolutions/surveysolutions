namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class NavigationModel
    {
        public NavigationModel(string text, string url)
        {
            this.Text = text;
            this.Url = url;
        }

        public string Url { get; set; }
        public string Text { get; set; }
    }
}
