using Android.Webkit;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class WebViewPdfFilePathBinding : BaseBinding<WebView, string>
    {
        public WebViewPdfFilePathBinding(WebView androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(WebView control, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                control.LoadUrl($"file:///android_asset/pdfjs/web/viewer.html?file=file://{value}");
            }
        }
    }
}
