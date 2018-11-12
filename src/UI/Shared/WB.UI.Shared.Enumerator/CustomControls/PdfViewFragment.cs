using System.IO;
using Android.OS;
using Android.Views;
using Android.Webkit;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Shared.Enumerator.CustomControls
{
    public class PdfViewFragment : BaseFragment<PdfViewModel>
    {
        private WebView webView;

        protected override int ViewResourceId => Resource.Layout.pdf_fragment;

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            webView = view.FindViewById<WebView>(Resource.Id.webView);
            webView.Settings.JavaScriptEnabled = true;
            webView.Settings.AllowFileAccessFromFileURLs = true;
            webView.Settings.AllowUniversalAccessFromFileURLs = true;
            webView.Settings.BuiltInZoomControls = true;
            webView.SetWebChromeClient(new WebChromeClient());
        }
    }
}
