using System.Collections.Specialized;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Java.Lang;
using MvvmCross.DroidX.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using WB.UI.Shared.Enumerator.Activities.Dashboard;

namespace WB.UI.Shared.Extensions.Activities;

public class MarkersRecyclerViewAdapter : MvxRecyclerAdapter
{
    private readonly IMvxAndroidBindingContext bindingContext;

    public MarkersRecyclerViewAdapter(IMvxAndroidBindingContext bindingContext) : base(bindingContext)
    {
        this.bindingContext = bindingContext;
    }

    public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
    {
        return base.OnCreateViewHolder(parent, viewType);
        //return base.OnCreateViewHolder(parent, Resource.Layout.marker_card);
        /*var itemBindingContext = new MvxAndroidBindingContext(parent.Context, this.bindingContext.LayoutInflaterHolder);
        var holder = this.InflateViewForHolder(parent, Resource.Layout.marker_card, itemBindingContext);
        return new RecyclerView.ViewHolder(holder);
        return base.OnCreateViewHolder(parent, viewType);*/
    }

    protected override void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsSourceCollectionChanged(sender, e);
    }
}
