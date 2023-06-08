using AndroidX.RecyclerView.Widget;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using WB.UI.Shared.Enumerator.Activities.Dashboard;
using Object = Java.Lang.Object;

namespace WB.UI.Shared.Extensions.Activities.Carousel;

public class CarouselViewAdapter : RecyclerViewAdapter
{
    public CarouselViewAdapter(IMvxAndroidBindingContext bindingContext) : base(bindingContext)
    {
    }

    public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position, IList<Object> payloads)
    {
        holder.ItemView.Tag = "position-" + position;
        base.OnBindViewHolder(holder, position, payloads);
    }

    protected override void OnItemViewClick(object sender, EventArgs e)
    {
        base.OnItemViewClick(sender, e);
    }
}