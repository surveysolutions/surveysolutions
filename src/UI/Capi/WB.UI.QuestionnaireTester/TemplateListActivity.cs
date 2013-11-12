using Android.App;
using Android.OS;
using WB.UI.QuestionnaireTester.Adapters;

namespace WB.UI.QuestionnaireTester
{
    [Activity(MainLauncher = true)]
    public class TemplateListActivity : ListActivity 
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.ListAdapter = new TemplateListAdapter(this);
        }
    }
}