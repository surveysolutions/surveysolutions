using AndroidX.ViewPager2.Widget;
using MvvmCross.Binding;

namespace WB.UI.Shared.Enumerator.CustomBindings;

public class ViewPager2CurrentItemBinding : BaseBinding<ViewPager2, int?>
{
    public ViewPager2CurrentItemBinding(ViewPager2 androidControl) : base(androidControl)
    {
    }

    protected override void SetValueToView(ViewPager2 control, int? value)
    {
        if (value.HasValue)
            control.SetCurrentItem(value.Value, true);
    }

    public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;
}