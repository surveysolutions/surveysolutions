using Android.Views;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class ViewPaddingStartBinding : BaseBinding<View, int>
    {
        public ViewPaddingStartBinding(View androidControl)
            : base(androidControl)
        {
        }

        protected override void SetValueToView(View control, int nodeDepth)
        {
            float dimension = control.Resources.GetDimension(Resource.Dimension.Interview_Sidebar_Group_margin_left);
            control.SetPaddingRelative((int)dimension * nodeDepth, control.PaddingTop, control.PaddingEnd, control.PaddingBottom);
        }
    }
}