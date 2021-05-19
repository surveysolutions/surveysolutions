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
        protected override int ViewResourceId => Resource.Layout.pdf_fragment;
    }
}
