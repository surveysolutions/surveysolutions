using Android.Views;

namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class ViewPaddingLeftBinding : BaseBinding<View, int>
    {
        public ViewPaddingLeftBinding(View androidControl)
            : base(androidControl)
        {
        }

        protected override void SetValueToView(View control, int nodeDepth)
        {
            float dimension = control.Resources.GetDimension(Resource.Dimension.Interview_Sidebar_Group_margin_left);
            control.SetPadding((int)dimension * nodeDepth, control.PaddingTop, control.PaddingRight, control.PaddingBottom);
        }
    }
}