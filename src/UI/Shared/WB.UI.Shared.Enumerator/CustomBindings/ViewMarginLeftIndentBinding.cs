using Android.Views;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class ViewMarginLeftIndentBinding : BaseBinding<View, int>
    {
        private int? baseMarginLeft;

        public ViewMarginLeftIndentBinding(View androidControl)
            : base(androidControl)
        {
        }

        protected override void SetValueToView(View control, int indent)
        {
            if (control.LayoutParameters is not ViewGroup.MarginLayoutParams layoutParams)
                return;

            if (baseMarginLeft == null)
                baseMarginLeft = layoutParams.LeftMargin;

            float step = control.Resources.GetDimension(Resource.Dimension.Interview_Question_indent_step);
            layoutParams.LeftMargin = baseMarginLeft.Value + (int)(indent * step);
            control.LayoutParameters = layoutParams;
        }
    }
}
