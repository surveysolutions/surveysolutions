using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Platforms.Android;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using WB.UI.Shared.Enumerator.Activities;
using Object = Java.Lang.Object;

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

        public override void OnViewDetachedFromWindow(Object holder)
        {
            // we do this, because same bindings use focus as triger, 
            // but in new version of MvvmCross focus event is raised after clear data in control
            bool isFocusedChildren = IsThereChildrenWithFocus(holder);
            if (isFocusedChildren) 
            {
                IMvxAndroidCurrentTopActivity topActivity = Mvx.Resolve<IMvxAndroidCurrentTopActivity>();
                var activity = topActivity.Activity;
                activity.RemoveFocusFromEditText();
            }

            base.OnViewDetachedFromWindow(holder);
        }

        private bool IsThereChildrenWithFocus(Object holder)
        {
            var viewHolder = holder as RecyclerView.ViewHolder;
            if (viewHolder != null)
                return IsThereChildrenWithFocus(viewHolder.ItemView);

            var view = holder as View;

            if (view == null)
                return false;

            if (view.IsFocused)
                return true;

            var viewGroup = view as ViewGroup;
            if (viewGroup != null)
                return IsThereChildrenWithFocus(viewGroup.FocusedChild);

            return false;
        }
    }
}
