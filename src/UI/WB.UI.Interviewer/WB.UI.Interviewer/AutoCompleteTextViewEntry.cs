using Xamarin.Forms;

namespace WB.UI.Interviewer
{
    public class AutoCompleteTextViewEntry : Xamarin.Forms.View
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly BindableProperty TextProperty = BindableProperty.Create<AutoCompleteTextViewEntry, string>(p => p.Text, string.Empty);
    }
}