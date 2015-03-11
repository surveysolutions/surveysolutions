using Xamarin.Forms;

namespace WB.UI.Interviewer.View
{
    public partial class InverviewDetails : ContentPage
    {
        public InverviewDetails()
        {
            InitializeComponent();
            BindingContext = App.Locator.InterviewDetails;
        }
    }
}
