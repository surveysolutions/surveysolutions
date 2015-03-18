using Android.Widget;
using WB.UI.Interviewer;
using WB.UI.Interviewer.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(AutoCompleteTextViewEntry), typeof(AutoCompleteTextViewRenderer))]

namespace WB.UI.Interviewer.Droid
{
    public class AutoCompleteTextViewRenderer : ViewRenderer<AutoCompleteTextViewEntry, AutoCompleteTextView>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<AutoCompleteTextViewEntry> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                var nativeControl = new AutoCompleteTextView(this.Context);
                nativeControl.SetPadding(10, 10, 10, 10);
                nativeControl.SetHeight(30);
                SetNativeControl(nativeControl);
            }

            if (Control != null && e.NewElement != null)
            {
                Control.Text = e.NewElement.Text;
            }
        }
    }
}