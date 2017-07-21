using Android.App;
using Android.Util;
using Android.Views;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class SizeInDpBinding : BaseBinding<View, int>
    {
        public SizeInDpBinding(View androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(View control, int value)
        {
            var valueInDpi = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, value, Application.Context.Resources.DisplayMetrics);

            var layout_params = control.LayoutParameters;
            layout_params.Height = valueInDpi;
            layout_params.Width = valueInDpi;
            control.LayoutParameters = layout_params;
        }
    }
}