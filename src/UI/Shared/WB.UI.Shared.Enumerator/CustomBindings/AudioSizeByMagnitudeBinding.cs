using Android.App;
using Android.Util;
using Android.Views;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class AudioSizeByMagnitudeBinding : BaseBinding<View, int>
    {
        private const int MinDpSize = 12;
        private const int MaxDpSize = 130;
        private const int MaxMagnitude = 20;
        private const double DpRatio = MaxDpSize / MaxMagnitude;

        public AudioSizeByMagnitudeBinding(View androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(View control, int value)
        {
            double dpValue = value * DpRatio;

            if (dpValue < MinDpSize)
                dpValue = MinDpSize;

            if (dpValue > MaxDpSize)
                dpValue = MaxDpSize;

            var valueInDpi = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, (int)dpValue, Application.Context.Resources.DisplayMetrics);

            var layout_params = control.LayoutParameters;
            layout_params.Height = valueInDpi;
            layout_params.Width = valueInDpi;
            control.LayoutParameters = layout_params;
        }
    }
}
 