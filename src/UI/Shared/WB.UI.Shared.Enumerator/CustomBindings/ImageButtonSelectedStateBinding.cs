using System.Windows.Input;
using AndroidX.Core.Content;
using AndroidX.Core.View;
using MvvmCross.Binding;
using MvvmCross.Commands;
using MvvmCross.WeakSubscription;

namespace WB.UI.Shared.Enumerator.CustomBindings;

public class ImageButtonSelectedStateBinding : BaseBinding<ImageButton, bool>
{
    public ImageButtonSelectedStateBinding(ImageButton androidControl) : base(androidControl) { }
    public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;
    protected override void SetValueToView(ImageButton control, bool value)
    {
        if (this.Target == null)
            return;
        Target.Selected = value;
    }
}
