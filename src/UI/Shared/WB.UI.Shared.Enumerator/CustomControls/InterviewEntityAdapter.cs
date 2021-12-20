using Android.Views;
using AndroidX.RecyclerView.Widget;
using MvvmCross.DroidX.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Shared.Enumerator.CustomControls
{
    public class InterviewEntityAdapter : MvxRecyclerAdapter
    {
        public InterviewEntityAdapter(IMvxAndroidBindingContext bindingContext)
            : base(bindingContext)
        {
        }

        public override void OnViewDetachedFromWindow(Java.Lang.Object holder)
        {
            // we do this, because same bindings use focus as trigger, 
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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
