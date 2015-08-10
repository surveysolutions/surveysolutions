using Cirrious.MvvmCross.Binding.BindingContext;

namespace WB.UI.Shared.Enumerator.CustomControls
{
    public interface IMvxRecyclerViewViewHolder : IMvxBindingContextOwner
    {
        object DataContext { get; set; }

        void OnAttachedToWindow();
        void OnDetachedFromWindow();
    }
}