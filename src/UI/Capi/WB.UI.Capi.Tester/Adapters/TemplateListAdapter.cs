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
using WB.UI.Shared.Android.Adapters;

namespace WB.UI.Capi.Tester.Adapters
{
    public class TemplateListAdapter:SmartAdapter<string>
    {
        private readonly Context context;
        public TemplateListAdapter(Context context)
            : base(new string[] { "Vegetables", "Fruits", "Flower Buds", "Legumes", "Bulbs", "Tubers" })
        {
            this.context = context;
        }

        protected override View BuildViewItem(string dataItem, int position)
        {
            LayoutInflater layoutInflater =
                (LayoutInflater) this.context.GetSystemService(Context.LayoutInflaterService);

            View view = layoutInflater.Inflate(Resource.Layout.template_list_item, null);
            var tvTitle =
                view.FindViewById<TextView>(Resource.Id.tvTitle);

            tvTitle.Text = dataItem;

            return view;
        }
    }
}