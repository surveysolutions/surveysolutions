using Android.Graphics;
using AndroidX.Core.Content;
using MvvmCross.Binding;
using WB.UI.Shared.Enumerator.Utils;

namespace WB.UI.Shared.Enumerator.CustomBindings;

public class TextViewColorBinding : BaseBinding<TextView, int?>
{
    public TextViewColorBinding(TextView androidControl) : base(androidControl) { }

    public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

    protected override void SetValueToView(TextView control, int? value)
    {
        if (!value.HasValue)
            return;

        var color = new Color(ContextCompat.GetColor(control.Context, value.Value));
        control.SetTextColor(color);
    }
}
