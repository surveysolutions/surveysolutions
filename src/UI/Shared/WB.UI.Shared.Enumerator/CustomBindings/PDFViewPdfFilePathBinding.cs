using Android.Webkit;
using PDFViewAndroid;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class PDFViewPdfFilePathBinding : BaseBinding<PDFView, string>
    {
        public PDFViewPdfFilePathBinding(PDFView androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(PDFView control, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                control.FromFile(value).Show();
            }
        }
    }
}
