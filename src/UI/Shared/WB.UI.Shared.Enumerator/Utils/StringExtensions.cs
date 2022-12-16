using Android.Text;

namespace WB.UI.Shared.Enumerator.Utils
{
    public static class StringExtensions
    {
        public static ISpanned ToAndroidSpanned(this string str)
        {
            ISpanned charSequence;
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.N)
            {
#pragma warning disable CA1416 // Validate platform compatibility
                charSequence = Html.FromHtml(str, FromHtmlOptions.ModeLegacy);
#pragma warning restore CA1416 // Validate platform compatibility
            }
            else
            {
#pragma warning disable 618
                charSequence = Html.FromHtml(str);
#pragma warning restore 618
            }

            return charSequence;
        }
    }
}
