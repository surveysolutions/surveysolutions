using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using AndroidX.Core.Content;
using MvvmCross.Binding;

namespace WB.UI.Shared.Enumerator.CustomBindings;

public class TextViewHintBinding : BaseBinding<TextView, int?>
{
    public TextViewHintBinding(TextView androidControl) : base(androidControl) { }

    public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

    protected override void SetValueToView(TextView control, int? value)
    {
        if (!value.HasValue)
            return;

        if (control.CompoundDrawableTintMode != PorterDuff.Mode.SrcIn)
            control.CompoundDrawableTintMode = PorterDuff.Mode.SrcIn;

        if (control.BackgroundTintMode != PorterDuff.Mode.SrcIn)
            control.BackgroundTintMode = PorterDuff.Mode.SrcIn;

        var color = new Color(ContextCompat.GetColor(control.Context, value.Value));
        //control.SetCompoundDrawablesWithIntrinsicBounds(drawable, null, null, null);

        if (control.Background is GradientDrawable drawable)
        {
            drawable.SetStroke(6, color); 
            drawable.SetAlpha(80);
            //drawable.SetColor(color);
        }        
         
        control.CompoundDrawableTintList = ColorStateList.ValueOf(color);
    }
}
