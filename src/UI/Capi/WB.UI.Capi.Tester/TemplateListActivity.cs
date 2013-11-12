namespace WB.UI.Capi.Tester
{
    using Android.App;
    using Android.OS;
    using WB.UI.Capi.Tester.Adapters;

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