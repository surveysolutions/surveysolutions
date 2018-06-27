using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross.Droid.Support.V7.RecyclerView;
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

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemBindingContext =
                new MvxAndroidBindingContext(parent.Context, this.bindingContext.LayoutInflaterHolder);
            var vh = new ExpandableViewHolder(this.InflateViewForHolder(parent, viewType, itemBindingContext),
                itemBindingContext)
            {
                Click = this.ItemClick,
                LongClick = this.ItemLongClick
            };

            return vh;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            base.OnBindViewHolder(holder, position);
            var viewHolder = (ExpandableViewHolder)holder;
            viewHolder.GetItem = () => this.GetItem(position);
        }
    }
}
