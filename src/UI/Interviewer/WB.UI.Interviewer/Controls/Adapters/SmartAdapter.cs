using System.Collections.Generic;
using Android.Views;
using Android.Widget;
using WB.UI.Interviewer.Extensions;

namespace WB.UI.Interviewer.Controls.Adapters
{
    public abstract class SmartAdapter<T> : BaseAdapter<T>
    {
        protected IList<T> items;

        protected SmartAdapter(IList<T> items)
        {
            this.items = items;
        }
        protected SmartAdapter()
        {
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var dataItem = this[position];

            if (convertView != null)
            {
                convertView.TryClearBindingsIfPossible();
            }

            convertView = this.CreateViewElement(dataItem, position);

            return convertView;
        }

        private View CreateViewElement(T dataItem, int position)
        {
            View convertView;
            convertView = this.BuildViewItem(dataItem, position);
           
            convertView.SetTag(Resource.Id.ElementId, position.ToString());
            return convertView;
        }

        protected abstract View BuildViewItem(T dataItem, int position);

        public override long GetItemId(int position)
        {
            return position;
        }

        public override int Count
        {
            get { return this.items.Count; }
        }

        public override T this[int position]
        {
            get { return this.items[position]; }
        }
    }
}