using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace CAPI.Android.Core
{
    public abstract class SmartAdapter<T> : BaseAdapter<T>
    {
        private readonly IList<T> items;

        protected SmartAdapter(IList<T> items)
        {
            this.items = items;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var dataItem = this[position];
            if (convertView == null)
            {
                convertView = CreateViewElement(dataItem, position);
            }
            else
            {
                var elementId = convertView.GetTag(Resource.Id.ElementId).ToString();
                if (elementId != GetElementFunction(dataItem).ToString())
                    convertView = CreateViewElement(dataItem, position);
            }
            return convertView;
        }

        private View CreateViewElement(T dataItem, int position)
        {
            View convertView;
            convertView = BuildViewItem(dataItem, position);
            convertView.SetTag(Resource.Id.ElementId, GetElementFunction(dataItem).ToString());
            return convertView;
        }

        protected abstract View BuildViewItem(T dataItem, int position);

        protected abstract object GetElementFunction(T dataItem);

        public override long GetItemId(int position)
        {
            return position;
        }

        public override int Count
        {
            get { return items.Count; }
        }

        public override T this[int position]
        {
            get { return items[position]; }
        }
    }
}