using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Platforms.Android;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Shared.Enumerator.CustomControls
{
    public class InterviewEntityAdapter : MvxRecyclerAdapter
    {
        private const int UnknownViewType = -1;

        public InterviewEntityAdapter(IMvxAndroidBindingContext bindingContext)
            : base(bindingContext)
        {
        }

        protected override View InflateViewForHolder(ViewGroup parent, int viewType, IMvxAndroidBindingContext bindingContext)
        {
            return viewType != UnknownViewType
                ? bindingContext.BindingInflate(viewType, parent, false)
                : this.CreateEmptyView(parent.Context);
        }

        private View CreateEmptyView(Context context)
        {
            return new View(context);
        }

        public override void OnViewDetachedFromWindow(Java.Lang.Object holder)
        {
            // we do this, because same bindings use focus as triger, 
            // but in new version of MvvmCross focus event is raised after clear data in control
            bool isFocusedChildren = IsThereChildrenWithFocus(holder);
            if (isFocusedChildren) 
            {
                var topActivity = Plugin.CurrentActivity.CrossCurrentActivity.Current.Activity;
                topActivity.RemoveFocusFromEditText();
            }

            base.OnViewDetachedFromWindow(holder);
        }

        private bool IsThereChildrenWithFocus(Java.Lang.Object holder)
        {
            if (holder is RecyclerView.ViewHolder viewHolder)
                return IsThereChildrenWithFocus(viewHolder.ItemView);

            var view = holder as View;

            if (view == null)
                return false;

            if (view.IsFocused)
                return true;

            if (view is ViewGroup viewGroup)
                return IsThereChildrenWithFocus(viewGroup.FocusedChild);

            return false;
        }
    }
}
