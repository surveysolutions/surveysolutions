using Android.Text;

namespace WB.UI.Shared.Enumerator.Utils
{
    public static class StringExtensions
    {
        public static ISpanned ToAndroidSpanned(this string str)
        {
            ISpanned charSequence;
#pragma warning disable CA1416 // Validate platform compatibility
            charSequence = Html.FromHtml(str, FromHtmlOptions.ModeLegacy);
#pragma warning restore CA1416 // Validate platform compatibility
            return charSequence;
        }
    }
}
