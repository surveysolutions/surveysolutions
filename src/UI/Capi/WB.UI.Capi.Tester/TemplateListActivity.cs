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
using WB.UI.Capi.Tester.Adapters;

namespace WB.UI.Capi.Tester
{
    [Activity(MainLauncher = true)]
    public class TemplateListActivity : ListActivity 
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            ListAdapter = new TemplateListAdapter(this);
        }
    }
}