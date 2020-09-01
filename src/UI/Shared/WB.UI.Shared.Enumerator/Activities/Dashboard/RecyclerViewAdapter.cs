using Android.Views;
using MvvmCross.DroidX.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;

namespace WB.UI.Shared.Enumerator.Activities.Dashboard
{
    public class RecyclerViewAdapter : MvxRecyclerAdapter
    {
        private readonly IMvxAndroidBindingContext bindingContext;

        public RecyclerViewAdapter(IMvxAndroidBindingContext bindingContext)
        {
            this.bindingContext = bindingContext;
        }

        public override AndroidX.RecyclerView.Widget.RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemBindingContext =
                new MvxAndroidBindingContext(parent.Context, this.bindingContext.LayoutInflaterHolder);
            var vh = new ExpandableViewHolder(this.InflateViewForHolder(parent, viewType, itemBindingContext),
                itemBindingContext);

            return vh;
        }

        public override void OnBindViewHolder(AndroidX.RecyclerView.Widget.RecyclerView.ViewHolder holder, int position)
        {
            base.OnBindViewHolder(holder, position);
            var viewHolder = (ExpandableViewHolder)holder;
            viewHolder.GetItem = () => this.GetItem(position);
        }
    }
}
