using Android.Graphics;
using Android.Views;
using AndroidX.Core.Content;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class ViewBackgroundColorBinding : BaseBinding<View, int?>
    {
        public ViewBackgroundColorBinding(View androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(View control, int? value)
        {
            if (!value.HasValue)
                return;

            var color = new Color(ContextCompat.GetColor(control.Context, value.Value));
            control.SetBackgroundColor(color);
        }
    }
}
