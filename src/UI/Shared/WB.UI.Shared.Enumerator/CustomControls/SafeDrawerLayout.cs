using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using AndroidX.DrawerLayout.Widget;
using WB.Core.GenericSubdomains.Portable.Services;
using MvvmCross;

namespace WB.UI.Shared.Enumerator.CustomControls
{
    /// <summary>
    /// Workaround for a rare framework/AndroidX bug where
    /// androidx.drawerlayout.widget.DrawerLayout#onMeasure() can return without calling
    /// setMeasuredDimension(), which crashes with:
    /// "IllegalStateException: ...DrawerLayout#onMeasure() did not set the measured dimension
    /// by calling setMeasuredDimension()".
    ///
    /// This subclass guarantees the measured dimension is always set, falling back to the
    /// requested measure spec size if the base implementation fails to do so (or throws).
    /// </summary>
    public class SafeDrawerLayout : DrawerLayout
    {
        public SafeDrawerLayout(Context context) : base(context)
        {
        }

        public SafeDrawerLayout(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public SafeDrawerLayout(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
        }

        protected SafeDrawerLayout(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            try
            {
                base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
            }
            catch (Exception ex)
            {
                LogFailure(ex);
            }

            // DrawerLayout is expected to always be measured with MeasureSpec.Exactly (it is
            // typically declared with match_parent width/height). Re-asserting the measured
            // dimension here is a no-op in the happy path (base already set the same values),
            // but guarantees setMeasuredDimension() is called even if the base implementation
            // hit the known edge case bug and returned/threw without setting it.
            var widthSize = MeasureSpec.GetSize(widthMeasureSpec);
            var heightSize = MeasureSpec.GetSize(heightMeasureSpec);
            SetMeasuredDimension(widthSize, heightSize);
        }

        private static void LogFailure(Exception ex)
        {
            try
            {
                Mvx.IoCProvider?.Resolve<ILoggerProvider>()?.GetForType(typeof(SafeDrawerLayout))
                    .Warn("DrawerLayout.OnMeasure threw an exception, falling back to measure spec size", ex);
            }
            catch
            {
                // logging must never crash the measure pass
            }
        }
    }
}
