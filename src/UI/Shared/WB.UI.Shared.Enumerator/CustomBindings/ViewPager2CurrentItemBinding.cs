﻿using Android.Views;
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
            var smoothScroll = control.Visibility != ViewStates.Invisible;
            if (smoothScroll)
            {
                control.Post(() =>
                {
                    control.SetCurrentItem(value.Value, true);
                });
            }
            else
            {
                control.SetCurrentItem(value.Value, false);
            }
        }
    }

    private OnPageChangeCallback onPageChangeCallback;

    public override void SubscribeToEvents()
    {
        var target = Target;
        if (target == null)
            return;

        onPageChangeCallback = new OnPageChangeCallback(index =>
        {
            this.FireValueChanged(index);
        });
        target.RegisterOnPageChangeCallback(onPageChangeCallback);
    }
    
    private class OnPageChangeCallback : ViewPager2.OnPageChangeCallback
    {
        private Action<int> action;

        public OnPageChangeCallback(Action<int> action)
        {
            this.action = action;
        }

        public override void OnPageSelected(int position)
        {
            action?.Invoke(position);
            base.OnPageSelected(position);
        }

        protected override void Dispose(bool disposing)
        {
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
