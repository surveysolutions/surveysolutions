using Android.App;
using Android.Util;
using Android.Views;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class AudioSizeByNoiseLevelBinding : BaseBinding<View, double>
    {
        private const int MinDpSize = 16;
        private const int MaxDpSize = 130;
        private const int MaxMagnitude = 90;
        private const int MinMagnitude = 20;
        private const double DpRatio = (double)MaxDpSize / (MaxMagnitude - MinMagnitude);

        public AudioSizeByNoiseLevelBinding(View androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(View control, double value)
        {
            if (control?.LayoutParameters == null) return;
            
            if (value > MaxMagnitude)
                value = MaxMagnitude;

            double dpValue = (value - MinMagnitude) * DpRatio;

            if (dpValue < MinDpSize)
                dpValue = MinDpSize;

            var valueInDpi = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, (int)dpValue, Application.Context.Resources.DisplayMetrics);

            var layout_params = control.LayoutParameters;
            layout_params.Height = valueInDpi;
            layout_params.Width = valueInDpi;
            control.LayoutParameters = layout_params;
        }
    }
}
 