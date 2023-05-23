using Android.Views;
using AndroidX.ConstraintLayout.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.ViewPager2.Widget;
using MvvmCross.Binding;
using MvvmCross.DroidX.RecyclerView;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;

namespace WB.UI.Shared.Enumerator.CustomBindings;

public class ViewPager2CurrentItemBinding : BaseBinding<ViewPager2, int?>
{
    public ViewPager2CurrentItemBinding(ViewPager2 androidControl) : base(androidControl)
    {
    }

    protected override void SetValueToView(ViewPager2 control, int? value)
    {
        if (value.HasValue && control.CurrentItem != value.Value)
        {
            control.Post(() =>
            {
                control.SetCurrentItem(value.Value, true);
            });
        }
    }

    private OnPageChangeCallback onPageChangeCallback;

    public override void SubscribeToEvents()
    {
        var target = Target;
        if (target == null)
            return;

        onPageChangeCallback = new OnPageChangeCallback(target, index =>
        {
            this.FireValueChanged(index);
        });
        target.RegisterOnPageChangeCallback(onPageChangeCallback);
    }
    
    private class OnPageChangeCallback : ViewPager2.OnPageChangeCallback
    {
        private ViewPager2 viewPager;
        private Action<int> action;
        private int? prevPosition;

        public OnPageChangeCallback(ViewPager2 viewPager, Action<int> action)
        {
            this.viewPager = viewPager;
            this.action = action;
        }

        public override void OnPageSelected(int position)
        {
            action?.Invoke(position);
            base.OnPageSelected(position);

            
            if (prevPosition.HasValue && prevPosition != position)
            {
                var recyclerView = viewPager.GetChildAt(0) as RecyclerView;
                var adapter = recyclerView?.GetAdapter() as MvxRecyclerAdapter;
                var dashboardItem = adapter?.GetItem(prevPosition.Value) as IDashboardItem;
                if (dashboardItem is { IsExpanded: true })
                    dashboardItem.IsExpanded = false;
            }

            prevPosition = position;
            
            
            var view = viewPager.FindViewWithTag("position-" + position);
            view?.Post(() =>
            {
                var wMeasureSpec = View.MeasureSpec.MakeMeasureSpec(view.Width, MeasureSpecMode.Exactly);
                var hMeasureSpec = View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);
                view.Measure(wMeasureSpec, hMeasureSpec);

                var pager = viewPager;

                if (pager?.LayoutParameters != null && pager.LayoutParameters.Height != view.MeasuredHeight)
                {
                    pager.LayoutParameters.Height = view.MeasuredHeight;
                    pager.RequestLayout();
                }
            });
        }

        protected override void Dispose(bool disposing)
        {
            viewPager = null;
            action = null;
            base.Dispose(disposing);
        }
    }

    public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            Target?.UnregisterOnPageChangeCallback(onPageChangeCallback);
            onPageChangeCallback?.Dispose();
        }
            
        base.Dispose(isDisposing);
    }
}