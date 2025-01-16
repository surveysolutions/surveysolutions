using System.Windows.Input;
using AndroidX.Core.Content;
using AndroidX.Core.View;
using MvvmCross.Binding;
using MvvmCross.Commands;
using MvvmCross.WeakSubscription;

namespace WB.UI.Shared.Enumerator.CustomBindings;

public class ImageButtonSelectedSupportClickBinding : BaseBinding<ImageButton, IMvxCommand>
{
    public ImageButtonSelectedSupportClickBinding(ImageButton androidControl) : base(androidControl) { }
    public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;
    protected override void SetValueToView(ImageButton control, IMvxCommand command)
    {
        if (this.Target == null)
            return;
        
        mvxCommand = (IMvxCommand)command;
    }

    private ICommand mvxCommand;
    private MvxWeakEventSubscription<ImageButton> clickedSubscription;
    
    public override void SubscribeToEvents()
    {
        var target = Target;
        if (target == null)
            return;
        
        clickedSubscription = target.WeakSubscribe(nameof(target.Click), this.ImageButtonClicked);
    }

    private void ImageButtonClicked(object sender, EventArgs e)
    {
        if (this.Target != null)
        {
            if (mvxCommand.CanExecute(Target.Selected))
            {
                mvxCommand.Execute(Target.Selected);
                Target.Selected = !Target.Selected;
            }
        }
    }

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            this.clickedSubscription?.Dispose();
            this.clickedSubscription = null;
        }
        
        base.Dispose(isDisposing);
    }
}
